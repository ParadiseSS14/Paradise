using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.PoweredClothing;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PoweredClothingComponent : Component
{
    [DataField]
    public float DrawRate = 0f;

    [DataField]
    public TimeSpan DrawTime = TimeSpan.FromSeconds(1f);

    [DataField]
    public bool SelfPowered = true;

    [DataField]
    [AutoNetworkedField]
    public EntityUid PowerSource;
}

[ByRefEvent]
public readonly record struct PoweredClothingTurnedOnEvent()
{
}

[ByRefEvent]
public readonly record struct PoweredClothingTurnedOffEvent()
{
}
