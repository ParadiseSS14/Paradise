namespace Content.Shared._Paradise.ChairArmrest;

[RegisterComponent]
public sealed partial class ChairArmrestComponent : Component
{
    [DataField]
    public required string ArmrestOverlay;

    public EntityUid? OverlayEntity;
}
