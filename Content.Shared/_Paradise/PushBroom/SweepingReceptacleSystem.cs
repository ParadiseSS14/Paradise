using Content.Shared.Containers.ItemSlots;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Unit;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;

namespace Content.Shared._Paradise.PushBroom;

/// <summary>
/// Puts swept items into whatever is willing to take them. A sweeping tool only has to find
/// a <see cref="SweepingReceptacleComponent"/>; working out whether that is a bag, a bin, or
/// a trolley carrying a bag is this system's problem.
/// </summary>
public sealed partial class SweepingReceptacleSystem : EntitySystem
{
    [Dependency] private SharedStorageSystem _storage = default!;
    [Dependency] private SharedDisposalUnitSystem _disposal = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;

    /// <summary>
    /// Tries to put <paramref name="item"/> into <paramref name="receptacle"/>, or into
    /// something the receptacle is carrying.
    /// </summary>
    /// <returns>False if nothing would take it, leaving the item for the sweeper to deal with.</returns>
    public bool TryDeposit(Entity<SweepingReceptacleComponent?> receptacle, EntityUid item, EntityUid? user, out EntityUid usedReceptacle)
    {
        usedReceptacle = default;

        if (!Resolve(receptacle.Owner, ref receptacle.Comp, false))
            return false;

        if (TryInsert(receptacle.Owner, item, user))
        {
            usedReceptacle = receptacle.Owner;
            return true;
        }

        if (receptacle.Comp.RelaySlots.Count == 0 || !TryComp<ItemSlotsComponent>(receptacle.Owner, out var slots))
            return false;

        foreach (var slotId in receptacle.Comp.RelaySlots)
        {
            if (!_itemSlots.TryGetSlot(receptacle.Owner, slotId, out var slot, slots)
                || slot.Item is not { } held
                || !TryInsert(held, item, user))
                continue;

            usedReceptacle = held;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Puts the item into one specific entity, whatever kind of container it happens to be.
    /// </summary>
    private bool TryInsert(EntityUid target, EntityUid item, EntityUid? user)
    {
        if (TryComp<DisposalUnitComponent>(target, out var disposal))
            return _disposal.TryInsert((target, disposal), item, user);

        // Insert does not re-check capacity or whitelists, so CanInsert has to come first.
        if (HasComp<StorageComponent>(target) && _storage.CanInsert(target, item, out _))
            return _storage.Insert(target, item, out _, user: user);

        return false;
    }
}
