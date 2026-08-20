namespace Content.Shared._Paradise.ChairArmrest;

[RegisterComponent]
public sealed partial class ChairArmrestComponent : Component
{
    [DataField]
    public required string ArmrestOverlay;

    [DataField]
    public bool ArmrestNoRot = false;

    public EntityUid? OverlayEntity;
}
