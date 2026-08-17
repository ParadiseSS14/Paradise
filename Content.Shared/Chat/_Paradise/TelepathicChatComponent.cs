using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Chat._Paradise;

/// <summary>
///   Telepathic chat component
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TelepathicChatComponent : Component
{
    /// <summary>
    /// String message to use when the sender is obscured
    /// </summary>
    public string ObscuredMessage;

    /// <summary>
    /// Token list of Offer Target and timeout timestamp
    /// </summary>
    public List<(EntityUid entity, TimeSpan timeout)> ReplyTokens = new();


    /// <summary>
    /// Dict for storage of data per action use, keyed on a "session" Guid
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Guid, TelepathyState> Sessions = new();

    /// <summary>
    /// List of TelepathyUIStates (sessions) used by the BUI
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<TelepathyUiState> UiKeySession = new();

    /// <summary>
    /// ID of the action prototype that allows you send messages
    /// </summary>
    [DataField]
    public EntProtoId? SendAction;

    /// <summary>
    /// ID of action prototype that allows you to receive messages
    /// </summary>
    [DataField]
    public EntProtoId? OfferAction;

    /// <summary>
    /// Entity of the send action prototype
    /// </summary>
    [DataField]
    public EntityUid? SendActionEntity;

    /// <summary>
    /// Entity of the receive action prototype
    /// </summary>
    [DataField]
    public EntityUid? OfferActionEntity;

    /// <summary>
    /// Send range float
    /// </summary>
    [DataField]
    public float Range = 14f;
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

/// <summary>
/// Stores values that should be unique to each use of an Action
/// </summary>
[Serializable, NetSerializable, DataDefinition]
public sealed partial class TelepathyState
{
    public NetEntity? Sender;
    public NetEntity? Receiver;
    public bool IsOffer;
    public TimeSpan Timeout;
    [DataField]
    public List<(NetEntity Uid, string Name)> TargetsList = new();
}

/// <summary>
/// Helps identify unique UI sessions without the Session ID
/// </summary>
[Serializable, NetSerializable, DataDefinition]
public sealed partial class TelepathyUiState
{
    [DataField]
    public TelepathicChatUiKey? UiKey;
    [DataField]
    public NetEntity? Actor;
    [DataField]
    public Guid SessionID;
}
