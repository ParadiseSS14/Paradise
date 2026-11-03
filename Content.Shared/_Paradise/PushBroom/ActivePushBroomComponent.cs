using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.PushBroom;

/// <summary>
/// Added to the user for as long as they are holding a wielded push broom,
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActivePushBroomComponent : Component
{
    /// <summary>
    /// The broom doing the sweeping.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Broom;
}
