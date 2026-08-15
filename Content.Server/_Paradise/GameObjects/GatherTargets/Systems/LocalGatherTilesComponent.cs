using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Map.Components;

namespace Content.Server._Paradise.GameObjects.GatherTargets;

public sealed partial class SameTileMappingSelectorSystem : EntitySystem
{
    [Dependency] private SharedMapSystem _mapSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LocalGatherTargetsComponent, GatherTargetsEvent>(OnGetTargets);
    }

    private bool TryLocal(EntityUid uid, [NotNullWhen(true)] out IEnumerable<EntityUid>? localEntities)
    {
        var transform = Transform(uid);
        var gridId = transform.GridUid;
        var coordinates = transform.Coordinates;
        if (!TryComp<MapGridComponent>(gridId, out var grid))
        {
            Log.Warning("Entitity {uid} is not on a grid.");
            localEntities = null;
            return false;
        }
        localEntities = _mapSystem.GetLocal(gridId.Value, grid, coordinates);
        return true;
    }

    private void OnGetTargets(EntityUid uid, LocalGatherTargetsComponent comp, ref GatherTargetsEvent args)
    {
        if (!TryLocal(uid, out var localEntities))
        {
            return;
        }
        // Add instead of setting so multiple selectors can be used at the same time.
        args.Targets.UnionWith(localEntities);
    }

}
