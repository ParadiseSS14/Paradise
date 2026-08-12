using Robust.Shared.GameStates;

namespace Content.Shared.Flash._Paradise.Components;

/// <summary>
/// This entity will take eye damage from flashes.
/// Copied from DamagedByFlashingComponent
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(BlindedByFlashingSystem))]
public sealed partial class BlindedByFlashingComponent : Component
{
    /// <summary>
    /// How much damage it will take.
    /// </summary>
    [DataField(required: true)]
    public int EyeDamage;
}
