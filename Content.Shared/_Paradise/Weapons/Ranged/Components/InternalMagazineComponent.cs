using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.Weapons.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InternalMagazineComponent : Component
{
    [DataField("magSlot")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string MagSlotId = "gun_magazine";

    [DataField]
    public ProtoId<ToolQualityPrototype> RequiredQuality = "Screwing";

    [DataField]
    public TimeSpan TimeToFix = new TimeSpan(0, 0, 10);

    [DataField]
    [AutoNetworkedField]
    public bool MagFixed = true;

    [DataField]
    [AutoNetworkedField]
    public bool MagDetachable = false;
}
