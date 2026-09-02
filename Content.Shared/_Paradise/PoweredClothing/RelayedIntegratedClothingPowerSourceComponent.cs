namespace Content.Shared._Paradise.PoweredClothing;

[RegisterComponent]
public sealed partial class RelayedIntegratedClothingPowerSourceComponent : Component
{
    [DataField]
    public string Slot = "back"; //Integrated item in this slot must have PowerCellSlotComponent
}
