using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.Weapons.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class GunAimableComponent : Component
{
    /// <summary>
    /// Whether the gun is currently being aimed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool IsAimed;

    /// <summary>
    /// Additive modifier applied to the gun's minimum spread angle while aimed.
    /// Negative values reduce the minimum spread.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("minAngle")]
    public Angle MinAngle = Angle.FromDegrees(0);

    /// <summary>
    /// Additive modifier applied to the gun's maximum spread angle while aimed.
    /// Negative values reduce the maximum spread.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxAngle")]
    public Angle MaxAngle = Angle.FromDegrees(0);

    /// <summary>
    /// Additive modifier applied to the gun's spread recovery rate while aimed.
    /// Positive values make the spread recover faster.
    /// </summary>
    [DataField]
    public Angle AngleDecay = Angle.FromDegrees(0);

    /// <summary>
    /// Additive modifier applied to the spread gained after each shot while aimed.
    /// Negative values reduce spread buildup.
    /// </summary>
    [DataField]
    public Angle AngleIncrease = Angle.FromDegrees(0);

    /// <summary>
    /// Sprint speed multiplier applied while aimed.
    /// A value of <see langword="null"/> leaves sprint speed unchanged.
    /// </summary>
    [DataField]
    public float? AimedSprintSpeedModifier = 0.5f;

    /// <summary>
    /// Walking speed multiplier applied while aimed.
    /// A value of <see langword="null"/> leaves walking speed unchanged.
    /// </summary>
    [DataField]
    public float? AimedWalkingSpeedModifier = 1f;
}
