using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Commands._Paradise;
using Content.Server.Chat.Managers;
using Content.Shared.Abilities.Mime;
using Content.Shared.Actions.Components;
using Content.Shared.Chat;
using Content.Shared.Chat._Paradise;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Server.Player;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.Chat._Paradise;

/// <summary>
/// Telepathy Ability System
/// </summary>
public sealed partial class TelepathyAbilitySystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IAdminManager _adminManager = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private IConfigurationManager _configManager = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelepathyAbilityComponent, SendTelepathyEvent>(OnSendEvent);
        SubscribeLocalEvent<TelepathyAbilityComponent, OfferTelepathyEvent>(OnOfferEvent);
        SubscribeLocalEvent<TelepathyAbilityComponent, TelepathyTargetSelectedMsg>(OnTargetChosen);
        SubscribeLocalEvent<TelepathyAbilityComponent, TelepathyTextEnteredMsg>(OnTextEntered);
    }

    private float _delayUpdate = 0f;
    private const float DelayInterval = 0.5f; // Only run Update twice a second
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _delayUpdate += frameTime;
        if (_delayUpdate < DelayInterval)
            return;

        _delayUpdate = 0f;
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<TelepathyAbilityComponent>();
        var expiryMessage = $"{Loc.GetString("chat-manager-telepathy-expiry")}";

        while (query.MoveNext(out var uid, out var telepathComp))
        {
            // Clearing out ReplyTokens after 10 seconds
            if (telepathComp.ReplyTokens.Count > 0)
            {
                for (var i = telepathComp.ReplyTokens.Count - 1; i >= 0; i--)
                {
                    var (entity, timeout) = telepathComp.ReplyTokens[i];
                    if (curTime < timeout)
                        continue;

                    telepathComp.ReplyTokens.RemoveAt(i);
                    if (GetClient(entity) is { } session)
                        _chatManager.ChatMessageToOne(ChatChannel.Hivemind, expiryMessage, expiryMessage, entity, false, session);
                }
            }

            // Clearing stale uiKeySessions with no UIs open
            foreach (var uiSession in telepathComp.UiKeySession.ToArray())
            {
                if (uiSession.UiKey is not { } key)
                    continue;

                if (uiSession.Actor == NetEntity.Invalid)
                    continue;

                if (_ui.IsUiOpen(uid, uiSession.UiKey, GetEntity(uiSession.Actor)))
                    continue;

                telepathComp.UiKeySession.Remove(uiSession);
            }

            // Clearing stale Sessions after 30 seconds
            foreach (var (sessionID, state) in telepathComp.Sessions.ToArray())
            {
                if (curTime < state.Timeout)
                    continue;

                telepathComp.Sessions.Remove(sessionID);
                telepathComp.UiKeySession.RemoveAll(x => x.SessionID == sessionID);
            }
        }
    }

    private void OnEvent(Entity<TelepathyAbilityComponent> telepath, EntityUid performer, Entity<ActionComponent> action)
    {
        if (!TryComp<UserInterfaceComponent>(telepath, out var userInterfaceComp))
            return;

        var sessionID = Guid.NewGuid();
        var validTargets = ChooseTargets(telepath.Owner, telepath.Comp.Range);
        telepath.Comp.Sessions[sessionID] = new TelepathyState
        {
            Sender = NetEntity.Invalid,
            Receiver = NetEntity.Invalid,
            IsOffer = false,
            Timeout = _timing.CurTime + TimeSpan.FromSeconds(30)
        };

        if (validTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("telepathy-no-targets"), telepath.Owner, telepath.Owner);
            telepath.Comp.Sessions.Remove(sessionID);
            return;
        }

        var uiKey = TelepathyAbilityUiKey.Send;

        if (action == telepath.Comp.OfferActionEntity) // Is this an offer?
        {
            uiKey = TelepathyAbilityUiKey.Offer;
            telepath.Comp.Sessions[sessionID].IsOffer = true;
        }

        var uiKeySession = new TelepathyUiState
        {
            UiKey = uiKey,
            Actor = GetNetEntity(performer),
            SessionID = sessionID
        };

        telepath.Comp.UiKeySession.Add(uiKeySession);
        telepath.Comp.Sessions[sessionID].ValidTargets = validTargets;
        var targetsList = new TelepathyTargetUIState(validTargets);

        _ui.OpenUi((telepath, userInterfaceComp), uiKey, performer);
        _ui.SetUiState((telepath, userInterfaceComp), uiKey, targetsList);
    }

    private void OnSendEvent(Entity<TelepathyAbilityComponent> telepath, ref SendTelepathyEvent args)
    {
        if (args.Handled)
            return;

        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
            return;

        telepath.Comp.ObscuredMessage = args.ObscuredMessage;
        OnEvent(telepath, args.Performer, args.Action);
        args.Handled = true;
    }

    private void OnOfferEvent(Entity<TelepathyAbilityComponent> telepath, ref OfferTelepathyEvent args)
    {
        if (args.Handled)
            return;

        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
            return;

        telepath.Comp.ObscuredMessage = args.ObscuredMessage;
        OnEvent(telepath, args.Performer, args.Action);
        args.Handled = true;
    }

    private void OnTargetChosen(Entity<TelepathyAbilityComponent> telepath, ref TelepathyTargetSelectedMsg args)
    {
        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
            return;

        if (!TryComp<UserInterfaceComponent>(telepath, out var userInterfaceComp))
            return;

        if (args.UiKey is not TelepathyAbilityUiKey key)
            return;

        if (args.Target is not { } target || args.Name is not { } name)
            return;

        if (!TryGetUiSession(telepath.Comp, key, GetNetEntity(args.Actor), out var session))
            return;

        var sessionID = session.SessionID;

        if (!telepath.Comp.Sessions.TryGetValue(sessionID, out var compSession))
            return;

        // Make sure the returned name and Uid pair existed in the original list
        if (!compSession.ValidTargets.Any(entry => entry.Uid == target && entry.Name == name))
            return;

        compSession.Sender = GetNetEntity(telepath.Owner);
        compSession.Receiver = target;
        compSession.TargetName = name;
        compSession.Timeout = _timing.CurTime + TimeSpan.FromSeconds(30);

        if (compSession.IsOffer)
        {
            SendTelepathyMessage(telepath, sessionID, string.Empty, false);
            return;
        }

        if (!CheckRange(telepath.Owner, GetEntity(target)))
        {
            telepath.Comp.Sessions.Remove(sessionID);
            return;
        }

        var uiKeySession = new TelepathyUiState
        {
            UiKey = TelepathyAbilityUiKey.Compose,
            Actor = GetNetEntity(args.Actor),
            SessionID = sessionID
        };

        telepath.Comp.UiKeySession.Add(uiKeySession);
        _ui.OpenUi((telepath, userInterfaceComp), TelepathyAbilityUiKey.Compose, args.Actor);
    }

    private void OnTextEntered(Entity<TelepathyAbilityComponent> telepath, ref TelepathyTextEnteredMsg args)
    {
        if (args.UiKey is not TelepathyAbilityUiKey key)
            return;

        if (!TryGetUiSession(telepath.Comp, key, GetNetEntity(args.Actor), out var session))
            return;

        var sessionID = session.SessionID;

        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
        {
            telepath.Comp.Sessions.Remove(sessionID);
            return;
        }

        var message = args.Message;
        SendTelepathyMessage(telepath, sessionID, message, false);
    }

    /// <summary>
    /// This method handles the response of Offer targets
    /// </summary>
    public void OpenComposeFor(EntityUid receiver, EntityUid sender)
    {
        if (!TryComp<TelepathyAbilityComponent>(sender, out var senderComp))
            return;

        if (!TryComp<UserInterfaceComponent>(sender, out var userInterfaceComp))
            return;

        // In case the telepath leaves PVS range before the link is clicked
        if (!CheckRange(sender, receiver))
        {
            _popup.PopupEntity(Loc.GetString("telepathy-target-left-range"), receiver, receiver);
            return;
        }

        // Oh boy, a new session of my very own!
        var sessionID = Guid.NewGuid();
        senderComp.Sessions[sessionID] = new TelepathyState
        {
            Sender = NetEntity.Invalid,
            Receiver = NetEntity.Invalid,
            IsOffer = false,
            IsOfferReply = true,
            Timeout = _timing.CurTime + TimeSpan.FromSeconds(30)
        };
        senderComp.Sessions[sessionID].Sender = GetNetEntity(receiver);
        senderComp.Sessions[sessionID].Receiver = GetNetEntity(sender);

        var uiKeySession = new TelepathyUiState
        {
            UiKey = TelepathyAbilityUiKey.Compose,
            Actor = GetNetEntity(receiver),
            SessionID = sessionID
        };

        senderComp.UiKeySession.Add(uiKeySession);
        _ui.OpenUi((sender, userInterfaceComp), TelepathyAbilityUiKey.Compose, receiver);
    }

    private List<(NetEntity Uid, string Name)> ChooseTargets(EntityUid telepath, float range)
    {
        var validTargets = new List<(NetEntity Uid, string Name)>(); //NetEntity is serializable
        var nearby = _lookup.GetEntitiesInRange<ActorComponent>(_transform.GetMapCoordinates(telepath), range);
        string mobName;

        foreach (var entity in nearby)
        {
            if (!_mobStateSystem.IsAlive(entity.Owner))
                continue;

            if (entity.Owner == telepath)
                continue;

            if (_interaction.InRangeUnobstructed(telepath, entity.Owner, range + 0.1f))
                mobName = Identity.Name(entity.Owner, EntityManager);
            else
                mobName = "Unknown entity";

            var resultName = mobName;
            var counter = 1;
            while (validTargets.Any(t => t.Name == resultName)) // Get rid of any duplicate names
            {
                resultName = $"{mobName} ({counter})";
                counter++;
            }

            validTargets.Add((GetNetEntity(entity.Owner), resultName));
        }

        return validTargets;
    }

    /// <summary>
    /// Check if telepath is outside max PVS range to enforce range limits and prevent OpenUI failure
    /// </summary>
    /// <remarks>
    ///  True = in range, false = outside range
    /// </remarks>
    private bool CheckRange(EntityUid telepath, EntityUid target)
    {
        var maxRange = _configManager.GetCVar(CVars.NetMaxUpdateRange);
        var telepathPos = _transform.GetMapCoordinates(telepath);
        var targetPos = _transform.GetMapCoordinates(target);

        return telepathPos.MapId == targetPos.MapId && (telepathPos.Position - targetPos.Position).Length() <= maxRange * 0.9f;
    }

    private bool TryGetUiSession(TelepathyAbilityComponent comp, TelepathyAbilityUiKey key, NetEntity actor, out TelepathyUiState session)
    {
        session = comp.UiKeySession.Find(x => x.UiKey == key && x.Actor == actor)!;

        if (session is null)
            return false;

        return true;
    }

    private IEnumerable<INetChannel> GetAdminClients()
    {
        return _adminManager.ActiveAdmins
            .Select(p => p.Channel);
    }

    private INetChannel? GetClient(EntityUid player)
    {
        if (_playerManager.TryGetSessionByEntity(player, out var session))
            return session.Channel;

        return null;
    }

    /// <summary>
    ///  Tries the token used by TelepathyReplyCommand
    /// </summary>
    public bool CheckOfferValid(EntityUid telepath, EntityUid target)
    {
        if (!TryComp<TelepathyAbilityComponent>(telepath, out var telepathComp))
            return false;

        if (!telepathComp.ReplyTokens.Exists(t => t.entity == target))
            return false;

        telepathComp.ReplyTokens.RemoveAll(t => t.entity == target);
        return true;
    }

    /// <summary>
    /// Sends reply sender and argument to the admin log
    /// </summary>
    public void LogInvalidReply(EntityUid sender, string rawArg)
    {
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Invalid telepathy_reply command from {ToPrettyString(sender):Player} with argument ({rawArg})");
    }

    /// <summary>
    /// Conditional build for message wraps
    /// </summary>
    private (string messageWrap, string sendMessage, string adminMessageWrap) BuildMessageWraps(
        TelepathyState session,
        string obscuredMessage,
        string rawMessage)
    {
        var sender = GetEntity(session.Sender);
        var receiver = GetEntity(session.Receiver);
        var targetName = session.TargetName;
        var isOffer = session.IsOffer;
        var isOfferReply = session.IsOfferReply;
        var message = FormattedMessage.EscapeText(rawMessage);

        // default wraps
        var messageWrap = $"{obscuredMessage} \"{message}\"";
        var sendMessage = Loc.GetString("chat-manager-send-telepathy-message", ("receiver", receiver));
        var offerMessageWrap = $"{Loc.GetString("chat-manager-telepathy-offer")}";
        var adminMessageWrap = Loc.GetString("chat-manager-receive-telepathy-wrap-message-admin", ("sender", sender), ("message", message));

        if (HasComp<TelepathyAbilityComponent>(sender)) // wraps for a telepath sender
            sendMessage = Loc.GetString("chat-manager-send-telepathy-message-telepath", ("receiver", targetName));

        if (HasComp<TelepathyAbilityComponent>(receiver)) // wraps for a telepath receiver
        {
            messageWrap = Loc.GetString("chat-manager-receive-telepathy-wrap-message-telepath", ("sender", sender), ("message", message));
            offerMessageWrap = Loc.GetString("chat-manager-telepathy-telepath", ("sender", sender));
        }

        if (isOffer) // wraps for an offer message
        {
            sendMessage = Loc.GetString("chat-manager-send-telepathy-message-offer", ("receiver", targetName));
            messageWrap = $"{offerMessageWrap} [cmdlink=\"{Loc.GetString("chat-manager-telepathy-link")}\" command=\"{TelepathyReplyCommand.CommandName} {GetNetEntity(sender)}\" /] ";
        }

        if (isOfferReply && HasComp<TelepathyAbilityComponent>(sender)) // Offer reply from telepath?
            sendMessage = Loc.GetString("chat-manager-send-telepathy-message-telepath", ("receiver", receiver));

        return (messageWrap, sendMessage, adminMessageWrap);
    }

    /// <summary>
    /// Send Telepathy messages to the Hivemind channel, logs, active admins
    /// </summary>
    public void SendTelepathyMessage(Entity<TelepathyAbilityComponent> telepath, Guid sessionID, string message, bool hideChat)
    {
        if (!TryComp<TelepathyAbilityComponent>(telepath, out var telepathComp))
            return;

        if (!telepathComp.Sessions.TryGetValue(sessionID, out var compSession))
            return;

        var sender = GetEntity(compSession.Sender);
        var receiver = GetEntity(compSession.Receiver);
        if (sender == EntityUid.Invalid || receiver == EntityUid.Invalid)
        {
            telepathComp.Sessions.Remove(sessionID);
            return;
        }

        if (TryComp<MimePowersComponent>(sender, out var mime) && !mime.VowBroken) // Mime Check
        {
            _popup.PopupEntity(Loc.GetString("mime-cant-speak"), sender, sender);
            telepathComp.Sessions.Remove(sessionID);
            return;
        }

        if (!CheckRange(sender, receiver))
        {
            _popup.PopupEntity(Loc.GetString("telepathy-target-left-range"), sender, sender);
            telepathComp.Sessions.Remove(sessionID);
            return;
        }

        var admins = GetAdminClients();
        var rxClient = GetClient(receiver);
        var txClient = GetClient(sender);

        if (rxClient is null || txClient is null) // Did they disconnect?
        {
            _popup.PopupEntity(Loc.GetString("telepathy-target-unreachable"), sender, sender);
            return;
        }

        // Get the sender's net session to make sure the message is within length limits
        if (TryComp<ActorComponent>(sender, out var actor) && _chatManager.MessageCharacterLimit(actor.PlayerSession, message))
            return;

        var (messageWrap, sendMessage, adminMessageWrap) = BuildMessageWraps(compSession, telepathComp.ObscuredMessage, message);

        if (compSession.IsOffer)
            telepathComp.ReplyTokens.Add((receiver, _timing.CurTime + TimeSpan.FromSeconds(10))); // Receiver and expiry of 10 seconds
        else //Not logging the linksend
        {
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Telepathy message from {ToPrettyString(sender):Player}: {message}"); // admin log
            _chatManager.ChatMessageToMany(ChatChannel.Admin, message, adminMessageWrap, sender, hideChat, true, admins); // message to active admins
        }

        _chatManager.ChatMessageToOne(ChatChannel.Hivemind, message, messageWrap, sender, hideChat, rxClient); // message to receiver
        _chatManager.ChatMessageToOne(ChatChannel.Hivemind, sendMessage, sendMessage, sender, hideChat, txClient); // message to sender

        telepath.Comp.Sessions.Remove(sessionID);
    }
}
