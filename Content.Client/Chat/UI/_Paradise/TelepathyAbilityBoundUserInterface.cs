using Content.Shared.Chat._Paradise;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Chat.UI._Paradise;
[UsedImplicitly]
public sealed partial class TelepathyAbilityBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private readonly Enum _uiKey = uiKey;
    private TelepathyAbilityMenu? _menu;
    private TelepathyAbilityComposeWindow? _composeWindow;
    private NetEntity? _targetEntity;
    private List<(NetEntity Uid, string Name)>? _targetsList;

    protected override void Open()
    {
        base.Open();

        if (UiKey.Equals(TelepathyAbilityUiKey.Send) || UiKey.Equals(TelepathyAbilityUiKey.Offer))
        {
            _menu = this.CreateWindow<TelepathyAbilityMenu>();
        }

        if (UiKey.Equals(TelepathyAbilityUiKey.Compose))
            _composeWindow = this.CreateWindow<TelepathyAbilityComposeWindow>();

        _menu?.OnSelectPressed += OnPressedSelect;
        _menu?.OnTargetSelected += OnSelectTarget;
        _menu?.OnTargetDeselected += OnDeselectTarget;
        _composeWindow?.OnTextEntered += OnEnteredText;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not TelepathyTargetUIState targetState)
            return;

        _targetsList = targetState.TargetsList;
        Reload();
    }

    private void OnPressedSelect()
    {
        if (_targetEntity is not { } targetEntity)
            return;

        SendPredictedMessage(new TelepathyTargetSelectedMsg(targetEntity));
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
        SendPredictedMessage(new TelepathyTextEnteredMsg(message));
        _composeWindow?.Close();
    }

    public void Reload()
    {
        if (_uiKey is not TelepathyAbilityUiKey)
            return;

        if (_targetsList is not { } targets || targets.Count == 0)
            return;

        _menu?.BuildList(targets);
    }
}
