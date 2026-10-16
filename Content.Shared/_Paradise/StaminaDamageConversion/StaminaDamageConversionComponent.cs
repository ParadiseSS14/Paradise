using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.StaminaDamageConversion;


[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StaminaDamageConversionComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public Dictionary<string, float> ConversionDict = new Dictionary<string, float> { { "Shock", 5f }, { "Blunt", 1.2f } };
}
