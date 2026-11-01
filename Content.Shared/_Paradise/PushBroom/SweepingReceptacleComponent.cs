namespace Content.Shared._Paradise.PushBroom;

/// <summary>
/// Marks an entity as somewhere a sweeping tool, such as a push broom, can dump what it
/// sweeps instead of shoving it onward across the floor.
/// </summary>
[RegisterComponent]
public sealed partial class SweepingReceptacleComponent : Component
{
    /// <summary>
    /// Item slots where containers that can be swept into are present if the entity holding the component doesn't have storage itself.
    /// This is neccesary for stuff like the jani trolly.
    /// </summary>
    [DataField]
    public List<string> RelaySlots = new();

    /// <summary>
    /// Order that this container is checked against others when a sweeping entity needs to figure out which container to deposit into first.
    /// </summary>
    [DataField]
    public int Priority;
}
