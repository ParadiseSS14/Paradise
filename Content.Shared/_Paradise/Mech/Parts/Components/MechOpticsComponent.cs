using Robust.Shared.GameStates;

namespace Content.Shared.Paradise.Mech.Parts.Components;

/// <summary>
/// Optics(a.k.a.eyes) of the mech
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MechOpticsComponent : Component
{
    [DataField]
    public Color CameraLayerColor = Color.Black;
}
