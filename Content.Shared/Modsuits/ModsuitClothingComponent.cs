using Robust.Shared.GameStates;

namespace Content.Shared.Modsuits.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModsuitClothingComponent : Component
{
    [DataField]
    public bool ValuesChanged = false;
    [DataField]
    public float LowPressureModifier { get; private set; } = 0f;
    [DataField]
    public float LowPressureMultiplier { get; private set; } = 1.0f;
    [DataField]
    public float HighPressureModifier { get; private set; } = 0f;
    [DataField]
    public float HighPressureMultiplier { get; private set; } = 1.0f;
    [DataField]
    public float CoefficientModifier { get; private set; } = 0f;
}
