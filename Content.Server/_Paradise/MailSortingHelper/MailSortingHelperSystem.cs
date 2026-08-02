using System.Diagnostics.CodeAnalysis;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Router;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._Paradise.MailSortingHelper;

public sealed partial class MailStoringHelperSystem : EntitySystem
{
    [Dependency] private SharedMapSystem _mapSystem = default!;
    [Dependency] private DisposalRouterSystem _routerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MailSortingHelperComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, MailSortingHelperComponent component, MapInitEvent args)
    {
        var transform = Transform(uid);
        var coordinates = transform.Coordinates;
        var gridId = transform.GridUid;

        if (!GetJunction(gridId, coordinates, out var router))
        {
            Log.Warning($"Mail Sorting Helper (UID{uid}) was placed on a tile at coordinates {coordinates} where no disposal router was found.");
            QueueDel(uid);
            return;
        }

        // Set the tags in the router to whatever is set in the helper's 'Mailtag'
        _routerSystem.SetTags(router.Value, component.Mailtag);
        QueueDel(uid);
    }

    private bool GetJunction(EntityUid? gridId, EntityCoordinates coordinates, [NotNullWhen(true)] out Entity<DisposalRouterComponent>? junction)
    {
        junction = null;
        // Are we on the grid?
        if (!TryComp<MapGridComponent>(gridId, out var grid))
            return false;

        // Go through all entities on the tile, if we find one with DisposalRouterComponent return true
        foreach (var entityUid in _mapSystem.GetLocal(gridId.Value, grid, coordinates))
        {
            if (!TryComp<DisposalRouterComponent>(entityUid, out var routerComponent))
                continue;

            junction = (entityUid, routerComponent);

            return true;
        }

        return false;
    }
}
