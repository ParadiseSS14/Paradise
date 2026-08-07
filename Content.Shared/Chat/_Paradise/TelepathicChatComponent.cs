using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat._Paradise;

/// <summary>
///   Telepathic chat component
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TelepathicChatComponent : Component
{
    /// <summary>
    /// Storage of the message sender
    /// </summary>
    public NetEntity? Sender;

    /// <summary>
    /// Storage of the message receiver
    /// </summary>
    public NetEntity? Receiver;


    /// <summary>
    /// Is this a Scan Mind Reply?
    /// </summary>
    public bool IsReply;

    /// <summary>
    /// The action prototype that allows you send messages
    /// </summary>
    [DataField]
    public EntProtoId SendAction = "ActionProjectMind";

    /// <summary>
    /// The action prototype that allows you to receive messages
    /// </summary>
    [DataField]
    public EntProtoId ReceiveAction = "ActionScanMind";

    /// <summary>
    /// Entities to hold the action prototypes
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SendActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ReceiveActionEntity;

    [DataField, AutoNetworkedField]
    public float Range;
}

public sealed partial class ProjectMindEvent : InstantActionEvent;

public sealed partial class ScanMindEvent : InstantActionEvent;