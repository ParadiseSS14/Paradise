using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Paradise.Mech.Equipment.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class MechEquipmentActionComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public EntityUid? EquipmentAbilityAction = null;

    /// <summary>
    /// Prototype of action this equpment provides
    /// </summary>
    [DataField]
    public EntProtoId? EquipmentAbilityActionName = null;
}
