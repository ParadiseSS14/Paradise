using Content.Shared.Chat._Paradise;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.Player;

namespace Content.Client.Chat.UI._Paradise;
[UsedImplicitly]
public sealed partial class TelepathicChatBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IPlayerManager _player = default!;

    [ViewVariables]
    private readonly EntityUid _owner = owner;
    private readonly Enum _uiKey = uiKey;
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

        if (UiKey.Equals(TelepathicChatUiKey.Compose))
            _composeWindow = this.CreateWindow<TelepathicChatComposeWindow>();

        _menu?.OnSelectPressed += OnPressedSelect;
        _menu?.OnTargetSelected += OnSelectTarget;
        _menu?.OnTargetDeselected += OnDeselectTarget;
        _composeWindow?.OnTextEntered += OnEnteredText;
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
        if (!EntMan.TryGetComponent<TelepathicChatComponent>(_owner, out var telepathComp))
            return;

        if (_uiKey is not TelepathicChatUiKey key)
            return;

        var actor = EntMan.GetNetEntity(_player.LocalSession?.AttachedEntity); // Pull the actor from the client session
        var uiSession = telepathComp.UiKeySession.Find(x => x.UiKey == key && x.Actor == actor);

        if (uiSession == default)
            return;

        if (!telepathComp.Sessions.TryGetValue(uiSession.SessionID, out var state))
            return;

        if (state.TargetsList.Count == 0)
            return;

        _menu?.BuildList(state.TargetsList);
    }
}
