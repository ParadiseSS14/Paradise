using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Chat._Paradise;

/// <summary>
///   Telepathic chat component
/// </summary>
[RegisterComponent, NetworkedComponent]
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
    public Dictionary<Guid, TelepathyState> Sessions = new();

    /// <summary>
    /// List of TelepathyUIStates (sessions) used by the BUI
    /// </summary>
    public List<TelepathyUiState> UiKeySession = new();

    /// <summary>
    /// ID of the action prototype that allows you send messages
    /// </summary>
    [DataField]
    public EntProtoId? SendAction;

    /// <summary>
    /// ID of action prototype that allows you to offer messages
    /// </summary>
    [DataField]
    public EntProtoId? OfferAction;

    /// <summary>
    /// Entity of the send action prototype
    /// </summary>
    [DataField]
    public EntityUid? SendActionEntity;

    /// <summary>
    /// Entity of the offer action prototype
    /// </summary>
    [DataField]
    public EntityUid? OfferActionEntity;

    /// <summary>
    /// Send range float
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float Range = 14f;
}

public sealed partial class SendTelepathyEvent : InstantActionEvent
{
    [DataField]
    public string ObscuredMessage;
}

public sealed partial class OfferTelepathyEvent : InstantActionEvent
{
    [DataField]
    public string ObscuredMessage;
}

/// <summary>
/// Stores values that should be unique to each use of an Action
/// </summary>
public sealed partial class TelepathyState
{
    public NetEntity? Sender;
    public NetEntity? Receiver;
    public bool IsOffer;
    public TimeSpan Timeout;
}

/// <summary>
/// Helps identify unique UI sessions without the Session ID
/// </summary>
public sealed partial class TelepathyUiState
{
    public TelepathicChatUiKey? UiKey;
    public NetEntity? Actor;
    public Guid SessionID;
}
