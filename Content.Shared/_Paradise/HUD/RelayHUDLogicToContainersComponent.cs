namespace Content.Shared._Paradise.RelayHUDLogic;

[RegisterComponent]

public sealed partial class RelayHUDLogicToContainersComponent : Component
{
    [DataField]
    public List<string> ContainerIDs = new List<string>();
}
