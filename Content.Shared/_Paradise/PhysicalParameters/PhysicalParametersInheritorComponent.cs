using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.PhysicalParameters;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class PhysicalParametersInheritorComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public HashSet<Parameter> ParametersToMove = new HashSet<Parameter>
    {
      { Parameter.ReactionSpeed},
      { Parameter.Coordination}
    };

    [DataField]
    [AutoNetworkedField]
    public bool AddParameters = false; //If false the parameters will be replaced 
}
