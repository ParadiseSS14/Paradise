namespace Content.Shared.Flash._Paradise.Components;

/// <summary>
/// This entity will take eye damage from flashes.
/// Copied from DamagedByFlashingComponent
/// </summary>
[RegisterComponent]
[Access(typeof(BlindedByFlashingSystem))]
public sealed partial class BlindedByFlashingComponent : Component
{
    /// <summary>
    /// How much damage it will take.
    /// </summary>
    /// <remarks>
    /// The maximum damage before total blindness in BlindableSystem is 9, so 3 flashes would blind if you set damage to 3. Two if you set it to 5, etc.  
    /// </remarks>
    [DataField(required: true)]
    public int EyeDamage;
}
