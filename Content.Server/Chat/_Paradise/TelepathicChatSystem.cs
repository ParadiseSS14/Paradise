using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Commands._Paradise;
using Content.Server.Chat.Managers;
using Content.Shared.Abilities.Mime;
using Content.Shared.Actions.Components;
using Content.Shared.Chat;
using Content.Shared.Chat._Paradise;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Server.Player;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server.Chat._Paradise;

/// <summary>
/// Telepathic Chat System
/// </summary>
public sealed partial class TelepathicChatSystem : EntitySystem
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

        SubscribeLocalEvent<TelepathicChatComponent, SendTelepathyEvent>(OnSendEvent);
        SubscribeLocalEvent<TelepathicChatComponent, OfferTelepathyEvent>(OnOfferEvent);
        SubscribeLocalEvent<TelepathicChatComponent, TelepathicTargetSelectedMsg>(OnTargetChosen);
        SubscribeLocalEvent<TelepathicChatComponent, TelepathicTextEnteredMsg>(OnTextEntered);
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
        var query = EntityQueryEnumerator<TelepathicChatComponent>();
        var expiryMessage = $"{Loc.GetString("chat-manager-telepathic-chat-expiry")}";

        while (query.MoveNext(out var uid, out var telepathComp))
        {
            var dirty = false;
            // Clearing out ReplyTokens after 10 seconds
            if (telepathComp.ReplyTokens.Count > 0)
            {
                for (var i = telepathComp.ReplyTokens.Count - 1; i >= 0; i--)
                {
                    var (entity, timeout) = telepathComp.ReplyTokens[i];
                    if (curTime < timeout)
                        continue;

                    telepathComp.ReplyTokens.RemoveAt(i);
                    dirty = true;
                    if (GetClient(entity) is { } session)
                        _chatManager.ChatMessageToOne(ChatChannel.Hivemind, expiryMessage, expiryMessage, entity, false, session);
                }
            }

            // Clearing stale uiKeySessions with no UIs open
            foreach (var uiSession in telepathComp.UiKeySession.ToArray())
            {
                if (uiSession.UiKey is not { } key)
                    continue;

                if (GetEntity(uiSession.Actor) is not { } actor)
                    continue;

                if (_ui.IsUiOpen(uid, uiSession.UiKey, actor))
                    continue;

                telepathComp.UiKeySession.Remove(uiSession);
                dirty = true;
            }

            // Clearing stale Sessions after 30 seconds
            foreach (var (sessionID, state) in telepathComp.Sessions.ToArray())
            {
                if (curTime < state.Timeout)
                    continue;

                telepathComp.Sessions.Remove(sessionID);
                telepathComp.UiKeySession.RemoveAll(x => x.SessionID == sessionID);
                dirty = true;
            }

            if (dirty)
                Dirty(uid, telepathComp);
        }
    }

    private void OnEvent(Entity<TelepathicChatComponent> telepath, EntityUid performer, Entity<ActionComponent> action)
    {
        if (!TryComp<UserInterfaceComponent>(telepath, out var userInterfaceComp))
            return;

        var sessionID = Guid.NewGuid();
        var validTargets = ChooseTargets(telepath.Owner, telepath.Comp.Range);
        telepath.Comp.Sessions[sessionID] = new TelepathyState
        {
            Sender = null,
            Receiver = null,
            IsOffer = false,
            Timeout = _timing.CurTime + TimeSpan.FromSeconds(30)
        };

        if (validTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("telepathic-chat-no-targets"), telepath.Owner, telepath.Owner);
            telepath.Comp.Sessions.Remove(sessionID);
            Dirty(telepath);
            return;
        }

        var uiKey = TelepathicChatUiKey.Send;

        if (action == telepath.Comp.ReceiveActionEntity) // Is this an offer?
        {
            uiKey = TelepathicChatUiKey.Receive;
            telepath.Comp.Sessions[sessionID].IsOffer = true;
        }

        var uiKeySession = new TelepathyUiState
        {
            UiKey = uiKey,
            Actor = GetNetEntity(performer),
            SessionID = sessionID
        };

        telepath.Comp.Sessions[sessionID].TargetsList = validTargets;
        telepath.Comp.UiKeySession.Add(uiKeySession);
        Dirty(telepath);

        _ui.OpenUi((telepath, userInterfaceComp), uiKey, performer);
    }

    private void OnSendEvent(Entity<TelepathicChatComponent> telepath, ref SendTelepathyEvent args)
    {
        if (args.Handled)
            return;

        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
            return;

        telepath.Comp.ObscuredMessage = args.ObscuredMessage;
        OnEvent(telepath, args.Performer, args.Action);
        args.Handled = true;
    }

    private void OnOfferEvent(Entity<TelepathicChatComponent> telepath, ref OfferTelepathyEvent args)
    {
        if (args.Handled)
            return;

        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
            return;

        telepath.Comp.ObscuredMessage = args.ObscuredMessage;
        OnEvent(telepath, args.Performer, args.Action);
        args.Handled = true;
    }

    private void OnTargetChosen(Entity<TelepathicChatComponent> telepath, ref TelepathicTargetSelectedMsg args)
    {
        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
            return;

        if (!TryComp<UserInterfaceComponent>(telepath, out var userInterfaceComp))
            return;

        if (args.UiKey is not TelepathicChatUiKey key)
            return;

        if (args.Target is not { } target)
            return;

        if (!TryGetUiSession(telepath.Comp, key, GetNetEntity(args.Actor), out var session))
            return;

        var sessionID = session.SessionID;

        if (!telepath.Comp.Sessions.TryGetValue(sessionID, out var compSession))
            return;

        compSession.Sender = GetNetEntity(telepath.Owner);
        compSession.Receiver = target;
        compSession.Timeout = _timing.CurTime + TimeSpan.FromSeconds(30);
        Dirty(telepath);

        if (compSession.IsOffer)
        {
            SendTelepathicChat(telepath, sessionID, string.Empty, false);
            return;
        }

        if (!CheckRange(telepath.Owner, GetEntity(target)))
        {
            telepath.Comp.Sessions.Remove(sessionID);
            Dirty(telepath);
            return;
        }

        var uiKeySession = new TelepathyUiState
        {
            UiKey = TelepathicChatUiKey.Compose,
            Actor = GetNetEntity(args.Actor),
            SessionID = sessionID
        };

        telepath.Comp.UiKeySession.Add(uiKeySession);
        _ui.OpenUi((telepath, userInterfaceComp), TelepathicChatUiKey.Compose, args.Actor);
    }

    private void OnTextEntered(Entity<TelepathicChatComponent> telepath, ref TelepathicTextEnteredMsg args)
    {
        if (args.UiKey is not TelepathicChatUiKey key)
            return;

        if (!TryGetUiSession(telepath.Comp, key, GetNetEntity(args.Actor), out var session))
            return;

        var sessionID = session.SessionID;

        if (telepath.Comp.LifeStage == ComponentLifeStage.Deleted)
        {
            telepath.Comp.Sessions.Remove(sessionID);
            Dirty(telepath);
            return;
        }

        var message = args.Message;
        SendTelepathicChat(telepath, sessionID, message, false);
    }

    /// <summary>
    /// This method handles the response of Offer targets
    /// </summary>
    public void OpenComposeFor(EntityUid receiver, NetEntity senderNet, EntityUid sender)
    {
        if (!TryComp<TelepathicChatComponent>(sender, out var senderComp))
            return;

        if (!TryComp<UserInterfaceComponent>(sender, out var userInterfaceComp))
            return;

        // In case the telepath leaves PVS range before the link is clicked
        if (!CheckRange(sender, receiver))
        {
            _popup.PopupEntity(Loc.GetString("telepathic-chat-target-left-range"), receiver, receiver);
            return;
        }

        // Oh boy, a new session of my very own!
        var sessionID = Guid.NewGuid();
        senderComp.Sessions[sessionID] = new TelepathyState
        {
            Sender = null,
            Receiver = null,
            IsOffer = false,
            Timeout = _timing.CurTime + TimeSpan.FromSeconds(30)
        };
        senderComp.Sessions[sessionID].Sender = GetNetEntity(receiver);
        senderComp.Sessions[sessionID].Receiver = senderNet;
        Dirty(sender, senderComp);

        var uiKeySession = new TelepathyUiState
        {
            UiKey = TelepathicChatUiKey.Compose,
            Actor = GetNetEntity(receiver),
            SessionID = sessionID
        };

        senderComp.UiKeySession.Add(uiKeySession);
        Dirty(sender, senderComp);
        _ui.OpenUi((sender, userInterfaceComp), TelepathicChatUiKey.Compose, receiver);
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
                mobName = Name(entity.Owner);
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

    private bool TryGetUiSession(TelepathicChatComponent comp, TelepathicChatUiKey key, NetEntity actor, out TelepathyUiState session)
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
    ///  Tries the token used by TelepathicChatReplyCommand
    /// </summary>
    public bool CheckOfferValid(EntityUid telepath, EntityUid target)
    {
        if (!TryComp<TelepathicChatComponent>(telepath, out var telepathComp))
            return false;

        if (!telepathComp.ReplyTokens.Exists(t => t.entity == target))
            return false;

        telepathComp.ReplyTokens.RemoveAll(t => t.entity == target);
        Dirty(telepath, telepathComp);
        return true;
    }

    /// <summary>
    /// Conditional build for message wraps
    /// </summary>
    private (string messageWrap, string sendMessage, string adminMessageWrap) BuildMessageWraps(
        TelepathicChatComponent telepathComp,
        EntityUid sender,
        EntityUid receiver,
        string message,
        bool isOffer)
    {
        // default wraps
        var messageWrap = $"{telepathComp.ObscuredMessage} \"{message}\"";
        var sendMessage = Loc.GetString("chat-manager-send-telepathic-chat-message", ("receiver", receiver));
        var offerMessageWrap = $"{Loc.GetString("chat-manager-telepathic-chat-offer")}";
        var adminMessageWrap = Loc.GetString("chat-manager-receive-telepathic-chat-wrap-message-admin", ("sender", sender), ("message", message));

        if (HasComp<TelepathicChatComponent>(sender)) // wraps for a telepath sender
            sendMessage = Loc.GetString("chat-manager-send-telepathic-chat-message-telepath", ("receiver", receiver));

        if (HasComp<TelepathicChatComponent>(receiver)) // wraps for a telepath receiver
        {
            messageWrap = Loc.GetString("chat-manager-receive-telepathic-chat-wrap-message-telepath", ("sender", sender), ("message", message));
            offerMessageWrap = Loc.GetString("chat-manager-telepathic-chat-telepath", ("sender", sender));
        }

        if (isOffer) // wraps for an offer message
        {
            sendMessage = Loc.GetString("chat-manager-send-telepathic-chat-message-offer", ("receiver", receiver));
            messageWrap = $"{offerMessageWrap} [cmdlink=\"{Loc.GetString("chat-manager-telepathic-chat-link")}\" command=\"{TelepathicChatReplyCommand.CommandName} {GetNetEntity(sender)}\" /] ";
        }

        return (messageWrap, sendMessage, adminMessageWrap);
    }

    /// <summary>
    /// Send Telepathic chats to the Hivemind channel, logs, active admins
    /// </summary>
    /// <remarks>
    /// Much of this has been sourced/referenced from Simple-Station/Einstein-Engine
    /// Commit: 10d41858d88d3ba9d36fdd9c98595d89701f1cbb
    /// Content.Server/Chat/TelepathicChatSystem.cs
    /// </remarks>
    public void SendTelepathicChat(Entity<TelepathicChatComponent> telepath, Guid sessionID, string message, bool hideChat)
    {
        if (!TryComp<TelepathicChatComponent>(telepath, out var telepathComp))
            return;

        if (!telepathComp.Sessions.TryGetValue(sessionID, out var compSession))
            return;

        var isOffer = compSession.IsOffer;

        if (GetEntity(compSession.Sender) is not { } sender || GetEntity(compSession.Receiver) is not { } receiver)
        {
            telepathComp.Sessions.Remove(sessionID);
            Dirty(telepath);
            return;
        }

        if (TryComp<MimePowersComponent>(sender, out var mime) && mime.VowBroken == false) // Mime Check
        {
            _popup.PopupEntity(Loc.GetString("mime-cant-speak"), sender, sender);
            telepathComp.Sessions.Remove(sessionID);
            Dirty(telepath);
            return;
        }

        if (!CheckRange(sender, receiver))
        {
            _popup.PopupEntity(Loc.GetString("telepathic-chat-target-left-range"), sender, sender);
            telepathComp.Sessions.Remove(sessionID);
            Dirty(telepath);
            return;
        }

        var admins = GetAdminClients();
        var rxClient = GetClient(receiver);
        var txClient = GetClient(sender);

        if (rxClient is null || txClient is null) // Did they disconnect?
        {
            _popup.PopupEntity(Loc.GetString("telepathic-chat-target-unreachable"), sender, sender);
            return;
        }

        var (messageWrap, sendMessage, adminMessageWrap) = BuildMessageWraps(telepathComp, sender, receiver, message, isOffer);

        if (isOffer)
            telepathComp.ReplyTokens.Add((receiver, _timing.CurTime + TimeSpan.FromSeconds(10))); // Receiver and expiry of 10 seconds
        else //Not logging the linksend
        {
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Telepathic chat from {ToPrettyString(sender):Player}: {message}"); // admin log
            _chatManager.ChatMessageToMany(ChatChannel.Admin, message, adminMessageWrap, sender, hideChat, true, admins); // message to active admins
        }

        _chatManager.ChatMessageToOne(ChatChannel.Hivemind, message, messageWrap, sender, hideChat, rxClient); // message to receiver
        _chatManager.ChatMessageToOne(ChatChannel.Hivemind, sendMessage, sendMessage, sender, hideChat, txClient); // message to sender

        telepath.Comp.Sessions.Remove(sessionID);
        Dirty(telepath);
    }
}
