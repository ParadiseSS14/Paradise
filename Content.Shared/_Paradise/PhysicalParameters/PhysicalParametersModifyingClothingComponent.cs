using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.PhysicalParameters;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class PhysicalParametersModifyingClothingComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public Dictionary<Parameter, FixedPoint2> ParameterDict = new Dictionary<Parameter, FixedPoint2>
    {
      { Parameter.Strength, 1}
    };

    [DataField]
    public bool DependsOnActivation = true;
}
