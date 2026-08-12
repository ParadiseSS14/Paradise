using Content.Client.Chat.UI._Paradise;
using Content.Shared.Chat._Paradise;

namespace Content.Client.Chat.Ui._Paradise;

public sealed partial class TelepathicChatSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelepathicChatComponent, AfterAutoHandleStateEvent>(OnAfterState);
    }

    private void OnAfterState(Entity<TelepathicChatComponent> telepath, ref AfterAutoHandleStateEvent args)
    {
        foreach (var key in Enum.GetValues<TelepathicChatUiKey>())
        {
            if (key is TelepathicChatUiKey.Compose) // Compose doesn't need TargetsList
                continue;

            if (!_ui.TryGetOpenUi<TelepathicChatBoundUserInterface>(telepath.Owner, key, out var bui))
                continue;

            bui.Reload();
        }
    }
}
