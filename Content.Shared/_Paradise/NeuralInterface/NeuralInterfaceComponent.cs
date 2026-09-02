using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Paradise.NeuralInterface;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class NeuralInterfaceComponent : Component
{
    [DataField, AutoNetworkedField]
    public int InterfaceCapacityRating = 10;

    [DataField]
    public ProtoId<AlertPrototype> InterfaceAlertProto = "NeuralInterface";

    [DataField, AutoNetworkedField]
    public int InterfaceType = 0;
}
