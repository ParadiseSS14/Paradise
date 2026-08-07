using Content.Shared.Chat._Paradise;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Chat.UI._Paradise;
[UsedImplicitly]
public sealed class TelepathicChatBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private TelepathicChatMenu? _menu;
    private TelepathicChatComposeWindow? _composeWindow;
    private NetEntity? _targetEntity;

    protected override void Open()
    {
        base.Open();

        if (UiKey.Equals(TelepathicChatUiKey.Send) || UiKey.Equals(TelepathicChatUiKey.Receive))
        {
            _menu = this.CreateWindow<TelepathicChatMenu>();
            Reload();
        }
        else
        {
            _composeWindow = this.CreateWindow<TelepathicChatComposeWindow>();
        }

        _menu?.SelectPressed += OnPressedSelect;
        _menu?.CancelPressed += OnCancel;
        _menu?.OnTargetSelected += OnSelectTarget;
        _menu?.OnTargetDeselected += OnDeselectTarget;
        _composeWindow?.OnTextEntered += OnEnteredText;
        _composeWindow?.CancelPressed += OnCancel;
    }
    private void OnPressedSelect()
    {
        if (_targetEntity is not { } targetEntity)
            return;

        SendPredictedMessage(new TelepathicTargetSelectedMsg(targetEntity));
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

    private void OnDeselectTarget()
    {
        _targetEntity = null;
    }

    private void OnEnteredText(String message)
    {
        SendPredictedMessage(new TelepathicTextEnteredMsg(message));
        _composeWindow?.Close();
    }

    public void Reload()
    {
        if (EntMan.TryGetComponent<TelepathicChatComponent>(Owner, out var comp))
        {
            _menu?.BuildList(comp.TargetsList);
        }
    }
}
