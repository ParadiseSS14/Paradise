using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.ItemExtension;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ItemExtensionComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public FixedPoint2 MinimalStrengthToPickUp = 1;

    [DataField]
    [AutoNetworkedField]
    public FixedPoint2 StrengthRequirementToBeUsed = 1;
}
