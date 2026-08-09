using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat._Paradise;

/// <summary>
///   Telepathic chat component
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
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
    /// Is this an OfferCompose?
    /// </summary>
    public bool IsOffer;

    /// <summary>
    /// Message to use when the sender is obscured
    /// </summary>
    public string ObscuredMessage;

    /// <summary>
    /// A token to handle command expiry
    /// </summary>
    public Guid? ReplyToken;

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
    [DataField]
    public EntityUid? SendActionEntity;

    [DataField]
    public EntityUid? ReceiveActionEntity;

    [DataField]
    public float Range;

    /// <summary>
    /// List of available targets for BUI state
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(NetEntity Uid, string Name)> TargetsList = new();

    public void Reset()
    {
        Sender = null;
        Receiver = null;
        IsOffer = false;
        ObscuredMessage = null;
    }
}

public sealed partial class SendTelepathyEvent : InstantActionEvent
{
    public readonly string ObscuredMessage;
}

public sealed partial class OfferTelepathyEvent : InstantActionEvent
{
    public readonly string ObscuredMessage;
}
