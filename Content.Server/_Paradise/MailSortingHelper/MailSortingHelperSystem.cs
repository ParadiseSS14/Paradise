using System.Diagnostics.CodeAnalysis;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Router;
using Content.Server._Paradise.GameObjects.GatherTargets;

namespace Content.Server._Paradise.MailSortingHelper;

public sealed partial class MailStoringHelperSystem : EntitySystem
{
    [Dependency] private DisposalRouterSystem _routerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MailSortingHelperComponent, MapInitEvent>(OnEvent);
    }

    private void OnEvent(Entity<MailSortingHelperComponent> entity, ref MapInitEvent args)
    {
        var xform = Transform(entity.Owner);
        var coordinates = xform.Coordinates;

        var targetEvent = new GatherTargetsEvent();
        RaiseLocalEvent(entity, ref targetEvent);
        var tileEntities = targetEvent.Targets;

        if (!GetJunction(tileEntities, out var router))
        {
            Log.Warning($"Mail Sorting Helper (UID{entity.Owner}) was placed on a tile at coordinates {coordinates} where no disposal router was found.");
            QueueDel(entity.Owner);
            return;
        }

        // Set the tags in the router to whatever is set in the helper's 'Mailtag'
        _routerSystem.SetTags(router.Value, entity.Comp.Mailtag);
        QueueDel(entity.Owner);
    }

    private bool GetJunction(IEnumerable<EntityUid> tileEntities, [NotNullWhen(true)] out Entity<DisposalRouterComponent>? junction)
    {
        junction = null;

        // Go through all entities on the tile, if we find one with DisposalRouterComponent return true
        foreach (var entityUid in tileEntities)
        {
            if (!TryComp<DisposalRouterComponent>(entityUid, out var routerComponent))
                continue;

            junction = (entityUid, routerComponent);

            return true;
        }

        return false;
    }
}
