using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Paradise.Mech.Equipment.Components;

/// <summary>
/// A piece of equipment that can be installed into <see cref="AltMechComponent"/>
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AltMechEquipmentComponent : Component
{
    /// <summary>
    /// How long does it take to install this piece of equipment
    /// </summary>
    [DataField("installDuration")] public float InstallDuration = 5;

    /// <summary>
    /// The mech that the equipment is inside of.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public EntityUid? EquipmentOwner = null;

    /// <summary>
    /// How much does this equipment weight
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public FixedPoint2 OwnMass = 0;

    /// <summary>
    /// How much space this equipment takes
    /// </summary>
    [DataField]
    public int EqipmentSize = 20;
}
