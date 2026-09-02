using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.AltArmor.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class AltArmorComponent : Component
{
    /// <summary>
    /// The damage tresholds(a.k.a. resists)
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> TresholdDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>();

    /// <summary>
    /// A list of armor damage tresholds(a.k.a. resist of the armor itself)
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> DurabilityTresholdDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>();

    /// <summary>
    /// Specifies what types of damage should be converted to others
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<DamageTypePrototype>, ProtoId<DamageTypePrototype>> TransformSpecifierDict = new Dictionary<ProtoId<DamageTypePrototype>, ProtoId<DamageTypePrototype>>();

    /// <summary>
    /// Does damage on this entity affect it's protection
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DamageAffectsProtection = false;//for now

    /// <summary>
    /// At which amount of damage taken does this entity looses all it's protection
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ZeroProtectionThreshold = 100;
}
