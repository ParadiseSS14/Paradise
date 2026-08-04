using Content.Shared.Chat._Paradise;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Chat.UI._Paradise;
// TODO: Need to find out how selection is confirmed when clicking "Select"
[UsedImplicitly]
public sealed class TelepathicChatBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private TelepathicChatMenu? _menu;
    private readonly TelepathicChatComposeWindow? _composeWindow;
    private NetEntity _targetEntity;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<TelepathicChatMenu>();
        _menu.SelectPressed += OnPressedSelect;
        _menu.CancelPressed += OnCancel;
        _menu.OnTargetSelected += OnSelectTarget;
        _composeWindow?.OnTextEntered += OnEnteredText;
        _composeWindow?.CancelPressed += OnCancel;

    }

    private void OnPressedSelect()
    {
        SendMessage(new TelepathicTargetSelectedMsg(_targetEntity));
        _menu?.Close();
    }

    private void OnCancel()
    {
        _menu?.Close();
        _composeWindow?.Close();
    }

    private void OnSelectTarget(NetEntity netEntity)
    {
        _targetEntity = netEntity;
    }

    private void OnEnteredText(String message)
    {
        SendMessage(new TelepathicTextEnteredMsg(message));
        _composeWindow?.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not TelepathicTargetsListState targetState)
            return;

        _menu?.UpdateState(targetState.Targets);
    }
}
