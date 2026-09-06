using Content.Shared.Chat._Paradise;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Chat.UI._Paradise;
[UsedImplicitly]
public sealed partial class TelepathicChatBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private readonly Enum _uiKey = uiKey;
    private TelepathicChatMenu? _menu;
    private TelepathicChatComposeWindow? _composeWindow;
    private NetEntity? _targetEntity;
    private List<(NetEntity Uid, string Name)>? _targetsList;

    protected override void Open()
    {
        base.Open();

        if (UiKey.Equals(TelepathicChatUiKey.Send) || UiKey.Equals(TelepathicChatUiKey.Offer))
        {
            _menu = this.CreateWindow<TelepathicChatMenu>();
        }

        if (UiKey.Equals(TelepathicChatUiKey.Compose))
            _composeWindow = this.CreateWindow<TelepathicChatComposeWindow>();

        _menu?.OnSelectPressed += OnPressedSelect;
        _menu?.OnTargetSelected += OnSelectTarget;
        _menu?.OnTargetDeselected += OnDeselectTarget;
        _composeWindow?.OnTextEntered += OnEnteredText;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not TelepathicTargetUIState targetState)
            return;

        _targetsList = targetState.TargetsList;
        Reload();
    }

    private void OnPressedSelect()
    {
        if (_targetEntity is not { } targetEntity)
            return;

        SendPredictedMessage(new TelepathicTargetSelectedMsg(targetEntity));
        _menu?.Close();
    }

    private void OnSelectTarget(NetEntity netEntity)
    {
        _targetEntity = netEntity;
    }

    private void OnDeselectTarget()
    {
        _targetEntity = null;
    }

    private void OnEnteredText(string message)
    {
        SendPredictedMessage(new TelepathicTextEnteredMsg(message));
        _composeWindow?.Close();
    }

    public void Reload()
    {
        if (_uiKey is not TelepathicChatUiKey)
            return;

        if (_targetsList is not { } targets || targets.Count == 0)
            return;

        _menu?.BuildList(targets);
    }
}
