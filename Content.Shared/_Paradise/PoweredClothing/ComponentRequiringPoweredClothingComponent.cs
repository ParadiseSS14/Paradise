using Content.Shared.Whitelist;

namespace Content.Shared._Paradise.PoweredClothing;

[RegisterComponent]
public sealed partial class ComponentRequiringPoweredClothingComponent : Component
{
    [DataField]
    public EntityWhitelist Whitelist = new()
    {
        Components = new[]
        {
            "NeuralInterface"
        }
    };
}
