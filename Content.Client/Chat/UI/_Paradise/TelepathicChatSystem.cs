using Content.Client.Chat.UI._Paradise;
using Content.Shared.Chat._Paradise;

namespace Content.Server.Client._Paradise;

public sealed partial class TelepathicChatSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelepathicChatComponent, BoundUIOpenedEvent>(OnOpened);
        SubscribeLocalEvent<TelepathicChatComponent, AfterAutoHandleStateEvent>(OnAfterState);
    }

    private void OnOpened(Entity<TelepathicChatComponent> telepath, ref BoundUIOpenedEvent args)
    {
        Reload(telepath.Owner);
    }

    private void OnAfterState(Entity<TelepathicChatComponent> telepath, ref AfterAutoHandleStateEvent args)
    {
        Reload(telepath.Owner);
    }

    private void Reload(EntityUid telepath)
    {
        if (!_uiSystem.TryGetOpenUi<TelepathicChatBoundUserInterface>(telepath, TelepathicChatUiKey.Send, out var sendbui))
        {
            sendbui?.Reload();
        }
        if (!_uiSystem.TryGetOpenUi<TelepathicChatBoundUserInterface>(telepath, TelepathicChatUiKey.Receive, out var recbui))
        {
            recbui?.Reload();
        }
    }
}