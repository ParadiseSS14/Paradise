using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Stacks;
using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.ComplexRepairable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ComplexRepairableComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier? Damage;

    [DataField, AutoNetworkedField]
    public FixedPoint2 FuelCost = 5;

    [DataField]
    public ProtoId<StackPrototype> Material;

    [AutoNetworkedField]
    public int LeftToInsert;

    [DataField("materialRepairTreshold"), AutoNetworkedField]
    public FixedPoint2 MaterialRepairTreshold;

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> QualityNeeded = "Welding";

    [DataField, AutoNetworkedField]
    public FixedPoint2 DoAfterModifier = 1;

    [DataField, AutoNetworkedField]
    public float SelfRepairPenalty = 3f;

    [DataField, AutoNetworkedField]
    public bool AllowSelfRepair = true;
}
