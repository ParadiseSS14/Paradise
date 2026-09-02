using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IntegratedClothingComponent : Component
{
    public const string DefaultClothingContainerId = "integrated-clothing";

    [DataField(required: true), AutoNetworkedField]
    public Dictionary<string, EntProtoId> ClothingPrototypes = new Dictionary<string, EntProtoId>();

    [DataField, AutoNetworkedField]
    public List<string> Slots = new List<string> { "head" };

    [DataField("requiredSlot"), AutoNetworkedField]
    public SlotFlags RequiredFlags = SlotFlags.OUTERCLOTHING;

    [DataField, AutoNetworkedField]
    public string ContainerId = DefaultClothingContainerId;

    [ViewVariables]
    public Dictionary<string, ContainerSlot> Containers = new Dictionary<string, ContainerSlot>();

    [AutoNetworkedField]
    public Dictionary<string, EntityUid> ClothingUids = new Dictionary<string, EntityUid>();
}
