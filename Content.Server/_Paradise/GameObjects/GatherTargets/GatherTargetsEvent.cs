namespace Content.Server._Paradise.GameObjects.GatherTargets;

/// Selector id exists in case of multiple components on the same entity sending such an event and needing different handlers.
[ByRefEvent]
public readonly record struct GatherTargetsEvent
{
    public readonly HashSet<EntityUid> Targets = new();
    public readonly string? SelectorId;

    public GatherTargetsEvent()
    {
    }
}
