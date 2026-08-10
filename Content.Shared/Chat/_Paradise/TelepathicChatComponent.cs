using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat._Paradise;

/// <summary>
///   Telepathic chat component
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
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
    /// Unique Guid and timestamp for Offer replies
    /// </summary>
    public Dictionary<EntityUid, (Guid token, TimeSpan timestamp)> ReplyToken = new();

    /// <summary>
    /// Message to use when the sender is obscured
    /// </summary>
    public string ObscuredMessage;

    /// <summary>
    /// List of available targets for BUI state
    /// </summary>
    [AutoNetworkedField]
    public List<(NetEntity Uid, string Name)> TargetsList = new();

    /// <summary>
    /// The action prototype that allows you send messages
    /// </summary>
    [DataField]
    public EntProtoId? SendAction;

    /// <summary>
    /// The action prototype that allows you to receive messages
    /// </summary>
    [DataField]
    public EntProtoId? ReceiveAction;

    /// <summary>
    /// Entity to hold the send action prototype
    /// </summary>
    [DataField]
    public EntityUid? SendActionEntity;

    /// <summary>
    /// Entity to hold the receive action prototype
    /// </summary>
    [DataField]
    public EntityUid? ReceiveActionEntity;

    /// <summary>
    /// Send range float
    /// </summary>
    [DataField]
    public float Range = 14f;

    /// <summary>
    /// Resets component state
    /// </summary>
    public void Reset()
    {
        Sender = null;
        Receiver = null;
        IsOffer = false;
    }
}

public sealed partial class SendTelepathyEvent : InstantActionEvent
{
    [DataField, AutoNetworkedField]
    public string ObscuredMessage;
}

public sealed partial class OfferTelepathyEvent : InstantActionEvent
{
    [DataField, AutoNetworkedField]
    public string ObscuredMessage;
}
