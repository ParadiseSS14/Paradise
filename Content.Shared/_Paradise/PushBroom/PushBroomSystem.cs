using Content.Shared.Containers.ItemSlots;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Unit;
using Content.Shared.IdentityManagement;
using Content.Shared.Item;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Wieldable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Numerics;

namespace Content.Shared._Paradise.PushBroom;

/// <summary>
/// Sweeps loose items along the floor when a wielded push broom is carried around.
/// </summary>
public sealed partial class PushBroomSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private ThrowingSystem _throw = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private SharedStorageSystem _storage = default!;
    [Dependency] private SharedDisposalUnitSystem _disposal = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    /// <summary>
    /// Reused tracking has of sweep items across calls so we are not allocating a new HashSet on every footstep.
    /// </summary>
    private readonly HashSet<Entity<ItemComponent>> _sweepBuffer = new();

    /// <summary>
    /// Separate buffer for scanning the destination tile, so we don't accidently iterate through stuff we're sweeping
    /// </summary>
    private readonly HashSet<EntityUid> _targetBuffer = new();
    [SubscribeLocalEvent]
    private void OnWielded(Entity<PushBroomComponent> ent, ref ItemWieldedEvent args)
    {
        var active = EnsureComp<ActivePushBroomComponent>(args.User);
        active.Broom = ent.Owner;
        Dirty(args.User, active);
    }

    [SubscribeLocalEvent]
    private void OnUnwielded(Entity<PushBroomComponent> ent, ref ItemUnwieldedEvent args)
    {
        RemComp<ActivePushBroomComponent>(args.User);
    }

    [SubscribeLocalEvent]
    private void OnUserMove(Entity<ActivePushBroomComponent> user, ref MoveEvent args)
    {
        // Turning or Changing grid/parent in place sweeps nothing.
        if (args.OnlyRotation || args.ParentChanged)
            return;

        // The broom may have been dropped, destroyed, or handed off
        if (!TryComp<PushBroomComponent>(user.Comp.Broom, out var broom))
        {
            RemComp<ActivePushBroomComponent>(user);
            return;
        }

        // We only care about crossing a tile boundary, otherwise a single step would sweep several times.
        if (TileOf(args.OldPosition.Position) == TileOf(args.NewPosition.Position))
            return;

        var xform = Transform(user); // get player TransformComponent
        var facing = xform.LocalRotation.GetCardinalDir(); // from that we can get the direction the player is facing

        // checking if a tile exists, if so, save TileRef which contains the data of the tile the user is standing on
        if (!_turf.TryGetTileRef(xform.Coordinates, out var standingOn))
            return;

        // get the map grid the user is standing on
        var gridUid = standingOn.Value.GridUid;
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        // This is the actual Vector2i location of the tile the user facing
        var destIndices = standingOn.Value.GridIndices.Offset(facing);
        // Now get tile data for the tile we're facing now that we know where it is
        var destTile = _map.GetTileRef((gridUid, grid), destIndices);

        // Never sweep rubbish out into open space.
        if (_turf.IsSpace(destTile))
            return;

        // Nor into a wall, a closed door, or anything else solid. Physics would stop
        // the item anyway, but bailing early avoids a pointless throw and sound.
        if (_turf.IsTileBlocked(destTile, CollisionGroup.Impassable))
            return;

        Sweep(
            user,
            (user.Comp.Broom.Value, broom),
            standingOn.Value.GridIndices,
            TileOf(args.OldPosition.Position),
            facing,
            destTile,
            gridUid);
    }

    /// <summary>
    /// What a swept item should be put into, if anything, on the destination tile.
    /// Both may be null, in which case items just get shoved onward.
    /// </summary>
    /// <remarks>
    /// one thing that makes this whole sweeping into receptables thing complicated is that we can sweep into both a disposals unit
    /// AND a bag which are two very distinct components, so we have to bifurcate our logic every time and this struct helps make that cleaner.
    /// </remarks>
    private readonly record struct Receptacles(
        EntityUid? TrashBag,
        Entity<DisposalUnitComponent>? Disposal);

    private bool IsTrashBag(EntityUid uid, PushBroomComponent broom) => _tag.HasAnyTag(uid, broom.TrashBagTags);

    /// <summary>
    /// Looks for somewhere on the destination tile to deposit swept items
    /// </summary>
    private Receptacles FindReceptacles(EntityUid gridUid, Vector2i destIndices, PushBroomComponent broom)
    {
        _targetBuffer.Clear();
        _lookup.GetLocalEntitiesIntersecting(gridUid, destIndices, _targetBuffer);

        EntityUid? bag = null;
        Entity<DisposalUnitComponent>? disposal = null;

        // start looking through the entities on the tile we're sweeping into
        foreach (var candidate in _targetBuffer)
        {
            // A disposal unit sitting on the tile.
            if (disposal == null && TryComp<DisposalUnitComponent>(candidate, out var disposalComp))
                disposal = (candidate, disposalComp);

            // A trash bag lying loose on the floor.
            if (bag == null && HasComp<StorageComponent>(candidate) && IsTrashBag(candidate, broom))
            {
                bag = candidate;
                continue;
            }

            // Or a trash bag stowed in a trolley's slot.
            if (bag == null
                && HasComp<ItemSlotsComponent>(candidate)
                && _itemSlots.TryGetSlot(candidate, broom.TrashBagSlot, out var slot)
                && slot.Item is { } stowed
                && IsTrashBag(stowed, broom))
            {
                bag = stowed;
            }
        }

        return new Receptacles(bag, disposal);
    }


    /// <summary>
    /// Tries to deposit a swept item into a bag or bin. Returns false if it should just
    /// be shoved along the floor instead.
    /// </summary>
    /// <param name="usedReceptacle">Which bag or bin actually took the item, for the popup.</param>
    private bool TryDeposit(EntityUid item, EntityUid user, Receptacles into, out EntityUid usedReceptacle)
    {
        usedReceptacle = default;

        // Bag first
        if (into.TrashBag is { } bag && _storage.CanInsert(bag, item, out _))
        {
            if (_storage.Insert(bag, item, out _, user: user))
            {
                usedReceptacle = bag;
                return true;
            }
        }

        if (into.Disposal is { } bin && _disposal.TryInsert(bin, item, user))
        {
            usedReceptacle = bin.Owner;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Pushes every loose item on <paramref name="sourceTile"/> one tile along <paramref name="facing"/>.
    /// </summary>
    private void Sweep(
        Entity<ActivePushBroomComponent> user,
        Entity<PushBroomComponent> broom,
        Vector2i sourceTile,
        Vector2i trailingTile,
        Direction facing,
        TileRef destTile,
        EntityUid gridUid)
    {
        // Everything on the tile we just stepped onto...
        _sweepBuffer.Clear();
        _lookup.GetLocalEntitiesIntersecting(gridUid, sourceTile, _sweepBuffer, flags: LookupFlags.Uncontained);

        // ...plus anything still sitting on the tile we just left. An item that did not
        // quite finish its slide lags a tile behind and would get missed in the next sweep.
        if (trailingTile != sourceTile)
            _lookup.GetLocalEntitiesIntersecting(gridUid, trailingTile, _sweepBuffer, flags: LookupFlags.Uncontained);


        // Is there a bag or a bin waiting on the destination tile?
        var receptacles = FindReceptacles(gridUid, destTile.GridIndices, broom.Comp);

        var stepVec = facing.ToVec();
        var destCentre = _turf.GetTileCenter(destTile).Position;
        var gridRotation = _transform.GetWorldRotation(gridUid);

        var centering = Math.Clamp(broom.Comp.CenteringStrength, 0f, 1f);

        // Number of entities we have "swept" in this action
        var swept = 0;

        // Where the rubbish ended up, if anywhere. Used later for popup string
        EntityUid? depositedInto = null;

        foreach (var item in _sweepBuffer)
        {
            if (swept >= broom.Comp.PushLimit)
                break;

            // Don't sweep the sweeper, or the broom out of their own hands.
            if (item.Owner == user.Owner || item.Owner == broom.Owner)
                continue;

            // Anchored items stay put
            var itemXform = Transform(item.Owner);
            if (itemXform.Anchored)
                continue;

            // Sweeping it into a bag or bin counts as sweeping it. Do this before any
            // of the direction maths, which is then wasted work.
            if (TryDeposit(item.Owner, user.Owner, receptacles, out var usedReceptacle))
            {
                swept++;
                depositedInto ??= usedReceptacle;
                continue;
            }

            Vector2 localDir;

            if (itemXform.ParentUid == gridUid) // item is on the ground
            {
                var itemPos = itemXform.Coordinates.Position;
                var straightAhead = itemPos + stepVec * broom.Comp.PushDistance;

                // Blend between "straight ahead" and "dead centre of the next tile".
                var target = Vector2.Lerp(straightAhead, destCentre, centering);
                localDir = target - itemPos;
            }
            else // item is on soemthing else, like a shelf, table, etc.
            {
                localDir = stepVec * broom.Comp.PushDistance;
            }

            // Rotate our grid-local vector to match.
            var direction = gridRotation.RotateVec(localDir);
            _throw.TryThrow(item.Owner, direction, broom.Comp.PushSpeed, user.Owner, compensateFriction: true, recoil: false, doSpin: false, animated: false, playSound: false);

            swept++;
        }

        // Nothing actually moved, so no sound and popup message.
        if (swept == 0)
            return;

        _audio.PlayPredicted(broom.Comp.SweepSound, broom.Owner, user.Owner);

        if (depositedInto is { } receptacle)
        {
            var self = Loc.GetString("push-broom-deposit-self",
                ("receptacle", receptacle));

            var others = Loc.GetString("push-broom-deposit-others",
                ("user", Identity.Entity(user, EntityManager)),
                ("receptacle", receptacle));

            _popup.PopupEntity(self, others, user, user);
        }
    }

    // helper function to get the tile coordinates of a position
    private static Vector2i TileOf(Vector2 position)
    {
        return new Vector2i(
            (int)MathF.Floor(position.X),
            (int)MathF.Floor(position.Y));
    }
}
