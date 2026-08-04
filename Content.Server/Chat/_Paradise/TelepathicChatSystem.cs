using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
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

    private List<(NetEntity Uid, string Name)> ChooseTargets(EntityUid sender, float range)
    {
        var validTargets = new List<(NetEntity Uid, string Name)>(); //NetEntity is serializable
        var nearby = _lookup.GetEntitiesInRange<ActorComponent>(_transform.GetMapCoordinates(sender), range);
        string mobName;

        foreach (var entity in nearby)
        {
            if (!_mobStateSystem.IsAlive(entity.Owner))
            {
                continue;
            }
            if (entity.Owner == sender)
            {
                continue;
            }
            if (_interaction.InRangeUnobstructed(sender, entity.Owner, range + 0.1f))
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

    private void OnSendEvent(Entity<TelepathicChatComponent> ent, ref ProjectMindEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(ent, out var userInterfaceComp))
        {
            return;
        }
        if (!_ui.IsUiOpen((ent, userInterfaceComp), TelepathicChatUiKey.Send, args.Performer))
        {
            _ui.OpenUi((ent, userInterfaceComp), TelepathicChatUiKey.Send, args.Performer);
        }

        var targets = ChooseTargets(ent.Owner, ent.Comp.Range);
        var state = new TelepathicTargetsListState(targets);

        _ui.SetUiState(ent.Owner, TelepathicChatUiKey.Send, state);
    }

    private void OnReceiveEvent(Entity<TelepathicChatComponent> ent, ref ScanMindEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(ent, out var userInterfaceComp))
        {
            return;
        }
        if (!_ui.IsUiOpen((ent, userInterfaceComp), TelepathicChatUiKey.Receive, args.Performer))
        {
            _ui.OpenUi((ent, userInterfaceComp), TelepathicChatUiKey.Receive, args.Performer);
        }

        var targets = ChooseTargets(ent.Owner, ent.Comp.Range);
        var state = new TelepathicTargetsListState(targets);

        _ui.SetUiState(ent.Owner, TelepathicChatUiKey.Receive, state);
    }

    private void OnTargetChosen(Entity<TelepathicChatComponent> ent, ref TelepathicTargetSelectedMsg args)
    {
        if (args.Target is not null)
        {
            ent.Comp.Target = args.Target; // Save target to Component

            if (!TryComp<UserInterfaceComponent>(ent, out var userInterfaceComp))
            {
                return;
            }
            if (!_ui.IsUiOpen((ent, userInterfaceComp), TelepathicChatUiKey.Compose, args.Actor))
            {
                _ui.OpenUi((ent, userInterfaceComp), TelepathicChatUiKey.Compose, args.Actor);
            }
        }
    }

    private void OnTextEntered(Entity<TelepathicChatComponent> ent, ref TelepathicTextEnteredMsg args)
    {

        if (ent.Comp.Target is not { } target)
            return;

        var targetentity = GetEntity(target);
        var message = args.Message;
        SendTelepathicChat(ent, targetentity, message, false);
    }


    private IEnumerable<INetChannel> GetAdminClients()
    {
        return _adminManager.ActiveAdmins
            .Select(p => p.Channel);
    }

    private INetChannel? GetTargetClients(EntityUid target)
    {
        if (_playerManager.TryGetSessionByEntity(target, out var session))
        {
            return session.Channel;
        }

        return null;
    }

    // TODO: Method to handle received target data
    private List<(NetEntity uid, string Name)> ReceiveMessage()
    {
        return new List<(NetEntity uid, string Name)>();
    }

    /// <summary>
    /// Much of this has been sourced/referenced from Simple-Station/Einstein-Engine 
    /// Commit: 10d41858d88d3ba9d36fdd9c98595d89701f1cbb
    /// Content.Server/Chat/TelepathicChatSystem.cs
    /// </summary>
    public void SendTelepathicChat(EntityUid source, EntityUid target, string message, bool hideChat)
    {
        var client = GetTargetClients(target);
        var admins = GetAdminClients();
        string messageWrap;
        string adminMessageWrap;

        messageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message",
            ("telepathicChannelName", Loc.GetString("chat-manager-telepathic-channel-name")), ("message", message));

        adminMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-admin",
            ("source", source), ("message", message));

        if (client is null)
        {
            _popup.PopupEntity(Loc.GetString("telepathic-chat-target-unreachable"), source, source);
        }
        else
        {
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Telepathic chat from {ToPrettyString(source):Player}: {message}");
            _chatManager.ChatMessageToOne(ChatChannel.Telepathic, message, messageWrap, source, hideChat, client, Color.DarkMagenta);
            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, adminMessageWrap, source, hideChat, true, admins, Color.DarkMagenta);
        }
    }
}
