using Content.Client.UserInterface.Fragments;
using Content.Shared.Paradise.Mech;
using Content.Shared.Paradise.Mech.Components;
using Content.Shared.Paradise.Mech.Systems;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Paradise.Mech.Ui;

[UsedImplicitly]
public sealed partial class AltMechBoundUserInterface : BoundUserInterface
{
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private SharedAltMechSystem _mech = default!;

    [ViewVariables]
    private AltMechMenu? _menu;

    public AltMechBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        if (!_ent.TryGetComponent<AltMechComponent>(Owner, out var mechComp))
            return;

        if (mechComp.Broken)
            return;

        base.Open();

        _menu = this.CreateWindowCenteredLeft<AltMechMenu>();


        _menu.SetEntity(Owner, MechPartVisualLayers.Core);

        foreach (var part in mechComp.ContainerDict)
        {
            _menu.SetEntity(part.Value.ContainedEntity, _mech.PartsVisuals[part.Key]);
        }

        _menu.OnRemovePartButtonPressed += part => SendMessage(new MechPartRemoveMessage(part));

        _menu.OnRemoveEquipmentButtonPressed += equipment => SendMessage(new AltMechEquipmentRemoveMessage(EntMan.GetNetEntity(equipment)));

        _menu.OnMaintenancePressed += toggled => SendMessage(new MechMaintenanceToggleMessage(toggled));

        _menu.OnBoltButtonPressed += _ => SendMessage(new MechBoltMessage(_));

        _menu.OnSealButtonPressed += _ => SendMessage(new MechSealMessage(_));

        _menu.OnDetachTankButtonPressed += _ => SendMessage(new MechDetachTankMessage(_));

        _menu?.UpdateMechStats();
        _menu?.SetupMechTankData();
        _menu?.UpdateEquipmentView();

        _menu?.SetMaintenance(mechComp.MaintenanceMode);
        _menu?.SetSeal(mechComp.Airtight);
        _menu?.SetBolt(mechComp.Bolted);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not AltMechBoundUiState msg)
            return;

        _menu?.UpdateMechStats();
        _menu?.UpdateMechTankData(msg);
        _menu?.UpdateEquipmentView();
    }

    public void UpdateUI()
    {
        _menu?.UpdateMechStats();
        _menu?.UpdateEquipmentView();
    }

    public UIFragment? GetEquipmentUi(EntityUid? uid)
    {
        var component = EntMan.GetComponentOrNull<UIFragmentComponent>(uid);
        component?.Ui?.Setup(this, uid);
        return component?.Ui;
    }
}

