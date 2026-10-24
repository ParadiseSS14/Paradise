using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;

namespace Content.Shared.NightVision;

/// <summary>
/// Shows/hides the <see cref="NightVisionOverlay"/> based on whether the observed
/// entity has a <see cref="NightVisionComponent"/> equipped.
/// </summary>
public abstract partial class SharedNightVisionSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;

    [Dependency] private BodySystem _body = default!;

    [SubscribeLocalEvent]
    private void OnStartup(Entity<NightVisionComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.RelayOverlay)
            return;

        RefreshOverlay(ent);
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    [SubscribeLocalEvent]
    private void OnRemove(Entity<NightVisionComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.RelayOverlay)
            return;

        RefreshOverlay(ent);
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    [SubscribeLocalEvent]
    private void OnCompEquip(Entity<NightVisionComponent> ent, ref GotEquippedEvent args)
    {
        if (!ent.Comp.RelayOverlay)
            return;

        RefreshOverlay(args.EquipTarget);
        _actions.AddAction(args.EquipTarget, ref ent.Comp.ActionEntity, ent.Comp.Action, ent);
    }

    [SubscribeLocalEvent]
    private void OnCompUnequip(Entity<NightVisionComponent> ent, ref GotUnequippedEvent args)
    {
        if (!ent.Comp.RelayOverlay)
            return;

        RefreshOverlay(args.EquipTarget);
    }

    [SubscribeLocalEvent]
    protected virtual void OnRefreshEquipmentHud(Entity<NightVisionComponent> ent, ref InventoryRelayedEvent<RefreshNightVisionEvent> args)
    {
        OnRefreshComponentHud(ent, ref args.Args);
    }

    [SubscribeLocalEvent]
    protected virtual void OnRefreshComponentHud(Entity<NightVisionComponent> ent, ref RefreshNightVisionEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        args.Entities.Add(ent);
    }
    
    /// <summary>
    /// Relays the refresh into the body's organs, so organs carrying a
    /// <see cref="NightVisionComponent"/> (Unathi eyes) count as a source.
    /// </summary>
    [SubscribeLocalEvent]
    private void OnRefreshBody(Entity<BodyComponent> ent, ref RefreshNightVisionEvent args)
    {
        _body.RelayEvent(ent, ref args);
    }

    [SubscribeLocalEvent]
    protected virtual void OnRefreshOrganHud(Entity<NightVisionComponent> ent, ref BodyRelayedEvent<RefreshNightVisionEvent> args)
    {
        if (!ent.Comp.Enabled)
            return;

        args.Args.Entities.Add(ent);
    }

    // Organs can be added or cut out mid-round, so the overlay has to be re-evaluated
    // when the body's contents change. Without these, losing your eyes keeps the effect
    // until something else happens to trigger a refresh.
    [SubscribeLocalEvent]
    private void OnOrganInserted(Entity<BodyComponent> ent, ref OrganInsertedIntoEvent args)
    {
        RefreshOverlay(ent);
    }

    [SubscribeLocalEvent]
    private void OnOrganRemoved(Entity<BodyComponent> ent, ref OrganRemovedFromEvent args)
    {
        RefreshOverlay(ent);
    }

    [SubscribeLocalEvent]
    private void OnToggleNightVisionEvent(ToggleNightVisionEvent args)
    {
        var ent = args.Action.Comp.Container;

        if (!TryComp<NightVisionComponent>(ent, out var nightVisionComp))
            return;

        SetEnabled(ent.Value, !nightVisionComp.Enabled, args.Performer);
        args.Handled = true;
    }

    /// <summary>
    /// Enables or disables the component.
    /// </summary>
    /// <param name="ent">The night vision to toggle.</param>
    /// <param name="enabled">Whether to enable or disable.</param>
    /// <param name="viewer">Viewer of the night vision, used to refresh their overlay. If null, assumes the night vision entity is the viewer.</param>
    public void SetEnabled(Entity<NightVisionComponent?> ent, bool enabled, EntityUid? viewer = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.Enabled == enabled)
            return;

        ent.Comp.Enabled = enabled;
        Dirty(ent);
        RefreshOverlay(viewer ?? ent);
    }

    protected virtual void RefreshOverlay(EntityUid entity) { }
}

[ByRefEvent]
public record struct RefreshNightVisionEvent() : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
    public List<Entity<NightVisionComponent>> Entities = new();
}
