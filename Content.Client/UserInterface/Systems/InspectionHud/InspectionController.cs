using Content.Client.Outline;
using Content.Client.UserInterface.Systems.Actions.Widgets;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.InspectionHud.UI;
using Content.Shared.IdentityManagement;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.InspectionHud;

public sealed partial class InspectionController : UIController
{
    [Dependency] private IEntitySystemManager _sysMan = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private InteractionOutlineSystem _system = default!;
    private InspectionUI? inspectionUI => UIManager.GetActiveUIWidgetOrNull<InspectionUI>();

    public override void Initialize()
    {

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnLoadSystem;
        gameplayStateLoad.OnScreenUnload += OnUnloadSystem;
    }

    public void OnLoadSystem()
    {
        _system = _sysMan.GetEntitySystem<InteractionOutlineSystem>();
        _system.InteractionOutlineCandidateChanged += ChangeInspectionText;
    }
    public void OnUnloadSystem()
    {
        _system.InteractionOutlineCandidateChanged -= ChangeInspectionText;
    }

    private void ChangeInspectionText(EntityUid? entity)
    {
        if (inspectionUI == null)
            return;

        if (!entity.HasValue)
        {
            inspectionUI.InspectionItemName.Text = string.Empty;
            return;
        }

        var itemName = FormattedMessage.EscapeText(Identity.Name(entity.Value, EntityManager, _playerManager.LocalEntity));

        inspectionUI.InspectionItemName.Text = itemName;

        inspectionUI.InspectionItemName.Align = Label.AlignMode.Center;
        inspectionUI.InspectionItemName.InvalidateArrange();
        inspectionUI.InspectionItemName.InvalidateMeasure();
    }
}
