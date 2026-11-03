using Content.Shared.Destructible.Thresholds;
using Content.Shared.EntityTable.EntitySelectors;


namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(BasicStationEventSchedulerSystem))]
public sealed partial class BasicStationEventSchedulerComponent : Component
{
    /// <summary>
    /// How long the the scheduler waits to begin starting rules.
    /// </summary>
    [DataField]
    public float MinimumTimeUntilFirstEvent = 200;

    /// <summary>
    /// How much additional time it may take for a GameRule to first start.
    /// </summary>
    [DataField]
    public float MaximumSpanUntilFirstEvent = 120;

    /// <summary>
    /// The minimum and maximum time between rule starts in seconds.
    /// </summary>
    [DataField]
    public MinMax MinMaxEventTiming = new(8 * 60, 20 * 60);

    /// <summary>
    /// How long until the next check for an event runs, is initially set based on MinimumTimeUntilFirstEvent & MinMaxEventTiming.
    /// </summary>
    [DataField]
    public float TimeUntilNextEvent;

    /// <summary>
    /// The gamerules that the scheduler can choose from
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector ScheduledGameRules = default!;
}
