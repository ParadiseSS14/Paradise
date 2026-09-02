namespace Content.Shared._Paradise.PoweredClothing;

[RegisterComponent]
public sealed partial class ActivePoweredClothingComponent : Component
{
    [DataField]
    public TimeSpan TargetTime = TimeSpan.Zero;
}
