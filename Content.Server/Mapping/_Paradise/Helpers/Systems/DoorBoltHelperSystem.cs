using System.Diagnostics.CodeAnalysis;
using Content.Server._Paradise.GameObjects.GatherTargets;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._Paradise.DoorBoltHelper;

public sealed partial class DoorBoltHelperSystem : EntitySystem
{
    [Dependency] private SharedDoorSystem _doorSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoorBoltHelperComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, DoorBoltHelperComponent component, MapInitEvent args)
    {
        var transform = Transform(uid);
        var coordinates = transform.Coordinates;

        var targetEvent = new GatherTargetsEvent();
        RaiseLocalEvent(uid, ref targetEvent);
        var tileEntities = targetEvent.Targets;

        if (!FindBoltableDoor(tileEntities, out var door))
        {
            Log.Warning($"Door Bolt Helper ({uid}) was unable to find boltable door at {coordinates}.");
            QueueDel(uid);
            return;
        }

        _doorSystem.ForceSetBoltsDown(door.Value);
        QueueDel(uid);
    }

    private bool FindBoltableDoor(IEnumerable<EntityUid> tileEntities,
        [NotNullWhen(true)] out Entity<DoorBoltComponent>? door)
    {
        door = null;
        foreach (var entityUid in tileEntities)
        {
            if (!TryComp<DoorBoltComponent>(entityUid, out var doorBoltComponent))
                continue;

            door = (entityUid, doorBoltComponent);

            return true;
        }

        return false;
    }
}
