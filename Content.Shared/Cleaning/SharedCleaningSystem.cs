using System.Linq;
using System.Numerics;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Decals;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Forensics.Systems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cleaning;

public sealed partial class SharedCleaningSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedDecalSystem _decals = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private SharedPopupSystem _popups = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CleaningComponent, AfterInteractEvent>(OnTryCleaning,
            after: [typeof(SharedAbsorbentSystem)]);
        SubscribeLocalEvent<CleaningComponent, CleaningDoAfterEvent>(DoCleaning);
    }

    private void OnTryCleaning(Entity<CleaningComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (ent.Comp.RequiresCleaningSolution)
        {
            if (!TryGetCleaningSolution(ent.Owner, ent.Comp.CleaningSolutionRequiredAmount, out _))
                return;
        }


        if (args.Target != null && HasComp<PuddleComponent>(args.Target))
            return;

        DoAfterArgs? doAfter;
        if (args.Target is { } target)
        {
            args.Handled = true;
            doAfter = new DoAfterArgs(
                EntityManager,
                args.User,
                ent.Comp.CleaningDuration,
                new CleaningDoAfterEvent(GetNetEntity(target)),
                ent,
                target)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
            };

            _doAfter.TryStartDoAfter(doAfter);
            return;
        }

        var gridUid = _transform.GetGrid(args.ClickLocation);

        if (gridUid is not { } grid ||
            !TryComp<MapGridComponent>(grid, out var mapGrid) ||
            !TryComp<DecalGridComponent>(grid, out var decalGrid))
            return;

        var snapPos = _map.TileIndicesFor((grid, mapGrid), args.ClickLocation);

        doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.CleaningDuration,
            new CleaningDoAfterEvent(GetNetEntity(grid), snapPos),
            ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
        };
        args.Handled = true;
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void DoCleaning(Entity<CleaningComponent> ent, ref CleaningDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (args.CleanTargetNetEntity is { } targetNet)
        {
            var cleanEnt = GetEntity(targetNet);

            // There are no blood decals from what I can see for taking damage, add the logic here if I am wrong.

            var ev = new CleaningFinishedEvent(
                ent.Owner,
                cleanEnt,
                args.Used);
            if (!TryComp(cleanEnt, out TransformComponent? transform))
                return;
            PredictedSpawnAttachedTo(ent.Comp.CleanedEffect, transform.Coordinates);
            RaiseLocalEvent(cleanEnt, ref ev);
            PostClean(ent, ref args);
            return;
        }

        if (args.GridNetEntity is not { } gridNet ||
            args.Tile is not { } tile)
            return;

        var cleaningStrength = ent.Comp.CleaningStrength;
        var gridUid = GetEntity(args.GridNetEntity.Value);

        if (!TryComp<DecalGridComponent>(gridUid, out var decalGrid))
            return;

        var tileCenter = new Vector2(args.Tile.Value.X + 0.5f, args.Tile.Value.Y + 0.5f);
        var decalsInRange = _decals.GetDecalsInRange(gridUid, tileCenter, 0.85f);
        var coords = new EntityCoordinates(gridUid, args.Tile.Value);

        var cleanedAnything = false;
        foreach (var decalSet in decalsInRange)
        {
            var decal = decalSet.Decal;

            if (decal == null || (decal.CleanType & cleaningStrength) == 0)
                continue;
            cleanedAnything = true;
            _decals.RemoveDecal(gridUid, decalSet.Index, decalGrid);
        }

        if (!cleanedAnything)
            return;
        PredictedSpawnAttachedTo(ent.Comp.CleanedEffect, coords);
        PostClean(ent, ref args);
    }

    private void PostClean(Entity<CleaningComponent> ent, ref CleaningDoAfterEvent args)
    {
        var comp = ent.Comp;
        if (comp.RequiresCleaningSolution)
        {
            if (!TryGetCleaningSolution(ent.Owner, comp.CleaningSolutionRequiredAmount, out var solution))
                return;

            if (args is { GridNetEntity: { } gridNet, Tile: { } tile })
            {
                var gridUid = GetEntity(gridNet);
                var coords = new EntityCoordinates(gridUid, new Vector2(tile.X + 0.5f, tile.Y + 0.5f));
                PredictedSpawnAttachedTo("WetFloorOverlay", coords);
            }

            solution?.RemoveSolution(comp.CleaningSolutionRequiredAmount);
        }

        if(comp.PlaysSound)
            _audio.PlayPredicted(comp.CleanSound, ent, args.User);
    }

    public ProtoId<ReagentPrototype>[] GetCleaningReagents(Solution solution, FixedPoint2 requiredAmount)
    {
        return solution
            .GetReagentPrototypes(ProtoMan)
            .Where(x => x.Key.Cleans && x.Value >= requiredAmount)
            .Select(x => (ProtoId<ReagentPrototype>)x.Key.ID)
            .ToArray();
    }

    private bool TryGetCleaningSolution(EntityUid uid, FixedPoint2 requiredAmount, out Solution? solution)
    {
        foreach (var solutionEntity in _solutionContainerSystem.EnumerateSolutions(uid))
        {
            var candidate = solutionEntity.Solution.Comp.Solution;
            if (GetCleaningReagents(candidate, requiredAmount).Length > 0)
            {
                solution = candidate;
                return true;
            }
        }
        solution = null;
        return false;
    }
}

[Serializable, NetSerializable]
public sealed partial class CleaningDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity? CleanTargetNetEntity { get; }
    public NetEntity? GridNetEntity { get; }
    public Vector2i? Tile { get; }


    public CleaningDoAfterEvent(NetEntity target)
    {
        CleanTargetNetEntity = target;
    }

    public CleaningDoAfterEvent(NetEntity grid, Vector2i tile)
    {
        GridNetEntity = grid;
        Tile = tile;
    }
}

[ByRefEvent]
public record struct CleaningFinishedEvent(
    EntityUid Cleaner,
    EntityUid? Target,
    EntityUid? Used);
