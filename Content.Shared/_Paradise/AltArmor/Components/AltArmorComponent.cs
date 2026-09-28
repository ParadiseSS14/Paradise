using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.AltArmor.Components;

public abstract partial class AltArmorComponent : Component
{
    /// <summary>
    /// The amount of damage blocked for the user, by damage type.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> UserDamageReductionByType = new();

    /// <summary>
    /// The amount of damage blocked when the armor itself takes damage, by damage type.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> SelfDamageReductionByType = new();

    /// <summary>
    /// Specifies what types of damage should be converted to others
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, ProtoId<DamageTypePrototype>> TransformSpecifierDict = new Dictionary<ProtoId<DamageTypePrototype>, ProtoId<DamageTypePrototype>>();

    /// <summary>
    /// Whether the protection provided by this entity decreases as the entity takes damage.
    /// </summary>
    [DataField]
    public bool DamageAffectsProtection = false;

    /// <summary>
    /// The total damage at which the protection provided by this entity is reduced to zero.
    /// Protection decreases linearly as the entity's damage approaches this value.
    /// </summary>
    [DataField]
    public int ZeroProtectionThreshold = 100;
}
