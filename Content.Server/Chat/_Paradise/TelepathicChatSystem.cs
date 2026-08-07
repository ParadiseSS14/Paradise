using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Commands._Paradise;
using Content.Server.Chat.Managers;
using Content.Shared.Actions.Components;
using Content.Shared.Chat;
using Content.Shared.Chat._Paradise;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Linq;

namespace Content.Server.Chat._Paradise;

/// <summary>
/// Telepathic Chat System
/// </summary>
public sealed partial class TelepathicChatSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IAdminManager _adminManager = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelepathicChatComponent, ProjectMindEvent>(OnSendEvent);
        SubscribeLocalEvent<TelepathicChatComponent, ScanMindEvent>(OnReceiveEvent);
        SubscribeLocalEvent<TelepathicChatComponent, TelepathicTargetSelectedMsg>(OnTargetChosen);
        SubscribeLocalEvent<TelepathicChatComponent, TelepathicTextEnteredMsg>(OnTextEntered);
    }

    private List<(NetEntity Uid, string Name)> ChooseTargets(EntityUid telepath, float range)
    {
        var validTargets = new List<(NetEntity Uid, string Name)>(); //NetEntity is serializable
        var nearby = _lookup.GetEntitiesInRange<ActorComponent>(_transform.GetMapCoordinates(telepath), range);
        string mobName;

        foreach (var entity in nearby)
        {
            if (!_mobStateSystem.IsAlive(entity.Owner))
            {
                continue;
            }
            if (entity.Owner == telepath)
            {
                continue;
            }
            if (_interaction.InRangeUnobstructed(telepath, entity.Owner, range + 0.1f))
            {
                mobName = Name(entity.Owner);
            }
            else
            {
                mobName = "Unknown entity";
            }

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

    private void OnEvent(Entity<TelepathicChatComponent> telepath, EntityUid performer, Entity<ActionComponent> action)
    {
        if (!TryComp<UserInterfaceComponent>(telepath, out var userInterfaceComp))
        {
            return;
        }

        telepath.Comp.Sender = null;
        telepath.Comp.Receiver = null;
        telepath.Comp.IsReply = false;
        Dirty(telepath);

        var targets = ChooseTargets(telepath.Owner, telepath.Comp.Range);
        if (targets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("telepathic-chat-no-targets"), telepath.Owner);
            return;
        }

        var state = new TelepathicTargetsListState(targets);
        var uiKey = TelepathicChatUiKey.Send;

        if (action == telepath.Comp.ReceiveActionEntity)
        {
            uiKey = TelepathicChatUiKey.Receive;
        }

        _ui.OpenUi((telepath, userInterfaceComp), uiKey, performer);
        _ui.SetUiState(telepath.Owner, uiKey, state);
    }
    private void OnSendEvent(Entity<TelepathicChatComponent> telepath, ref ProjectMindEvent args)
    {
        OnEvent(telepath, args.Performer, args.Action);
    }

    private void OnReceiveEvent(Entity<TelepathicChatComponent> telepath, ref ScanMindEvent args)
    {
        OnEvent(telepath, args.Performer, args.Action);
    }

    private void OnTargetChosen(Entity<TelepathicChatComponent> telepath, ref TelepathicTargetSelectedMsg args)
    {
        if (args.Target is not { } target)
        {
            return;
        }



        if (Equals(args.UiKey, TelepathicChatUiKey.Receive))
        {
            SendTelepathicChat(telepath.Owner, GetEntity(target), string.Empty, false, replyWrap: true);

            telepath.Comp.Receiver = null;
            telepath.Comp.IsReply = false;
            Dirty(telepath);
            return;
        }
        else if (TryComp<UserInterfaceComponent>(telepath, out var userInterfaceComp))
        {
            telepath.Comp.Receiver = args.Target; // Save receiver to Component
            Dirty(telepath);

            _ui.OpenUi((telepath, userInterfaceComp), TelepathicChatUiKey.Compose, args.Actor);
        }
    }

    private void OnTextEntered(Entity<TelepathicChatComponent> telepath, ref TelepathicTextEnteredMsg args)
    {
        if (telepath.Comp.Receiver is not { } receiverNet)
        {
            return;
        }

        var receiverEnt = GetEntity(receiverNet);
        var message = args.Message;

        if (telepath.Comp.IsReply)
        {
            SendTelepathicChat(telepath, receiverEnt, message, false, true);
            telepath.Comp.Sender = receiverNet; // Swap target to sender for response
            telepath.Comp.IsReply = false;
        }
        else
        {
            SendTelepathicChat(telepath, receiverEnt, message, false, false);
        }

        telepath.Comp.Receiver = null;
        Dirty(telepath);
    }

    /// <summary>
    /// This method handles the response of ScanMind targets
    /// </summary>
    public void OpenComposeFor(EntityUid telepath, EntityUid target, NetEntity telepathNet)
    {
        if (!TryComp<UserInterfaceComponent>(telepath, out var userInterfaceComp))
        {
            return;
        }

        if (!TryComp<TelepathicChatComponent>(telepath, out var telepathComp))
            return;

        telepathComp.Receiver = telepathNet; // Save receiver to Component
        telepathComp.IsReply = true; // Mark as reply message
        Dirty(telepath, telepathComp);

        _ui.OpenUi((telepath, userInterfaceComp), TelepathicChatUiKey.Compose, target);
    }


    private IEnumerable<INetChannel> GetAdminClients()
    {
        return _adminManager.ActiveAdmins
            .Select(p => p.Channel);
    }

    private INetChannel? GetReceiverClients(EntityUid receiver)
    {
        if (_playerManager.TryGetSessionByEntity(receiver, out var session))
        {
            return session.Channel;
        }

        return null;
    }

    /// <summary>
    /// Much of this has been sourced/referenced from Simple-Station/Einstein-Engine 
    /// Commit: 10d41858d88d3ba9d36fdd9c98595d89701f1cbb
    /// Content.Server/Chat/TelepathicChatSystem.cs
    /// </summary>
    public void SendTelepathicChat(EntityUid sender, EntityUid receiver, string message, bool hideChat, bool replyWrap = false)
    {
        var rxClient = GetReceiverClients(receiver);
        var admins = GetAdminClients();
        string replyMessage;
        string messageWrap;
        string adminMessageWrap;

        replyMessage = $"{Loc.GetString("chat-manager-telepathic-chat-scan")}";
        messageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message",
            ("sender", sender), ("message", message));
        adminMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-admin",
            ("sender", sender), ("message", message));

        if (replyWrap)
        {
            messageWrap = $"{replyMessage} [cmdlink=\"{Loc.GetString("chat-manager-telepathic-chat-reply")}\" command=\"{TelepathicChatReplyCommand.CommandName} {GetNetEntity(sender)}\" /] ";
        }

        if (rxClient is null)
        {
            _popup.PopupEntity(Loc.GetString("telepathic-chat-target-unreachable"), sender, sender);
        }
        else
        {
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Telepathic chat from {ToPrettyString(sender):Player}: {message} {messageWrap}");
            _chatManager.ChatMessageToOne(ChatChannel.Telepathic, message, messageWrap, sender, hideChat, rxClient, Color.DarkMagenta);
            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, adminMessageWrap, sender, hideChat, true, admins, Color.DarkMagenta);
        }
    }
}
