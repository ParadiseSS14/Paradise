using System.Diagnostics.CodeAnalysis;
using Content.Server._Paradise.GameObjects.GatherTargets;
using Content.Shared.Access.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._Paradise.AccessHelper
{
    public sealed partial class AccessHelperSystem : EntitySystem
    {
        [Dependency] private AccessReaderSystem _accessReaderSystem = default!;
        [Dependency] private TagSystem _tagSystem = default!;

        private static readonly ProtoId<TagPrototype> TagWindoor = "Windoor";
        private static readonly ProtoId<TagPrototype> TagWindoorHelper = "WindoorHelper";
        private readonly List<(EntityUid helper, Entity<AirlockComponent> door) > _pending = new();

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AccessHelperComponent, MapInitEvent>(OnEvent);
        }

        private void OnEvent(Entity<AccessHelperComponent> entity, ref MapInitEvent args)
        {
            var transform = Transform(entity.Owner);
            var coordinates = transform.Coordinates;
            var helperAngle = transform.LocalRotation;
            var isWindoorHelper = _tagSystem.HasTag(entity.Owner, TagWindoorHelper);
            var targetEvent = new GatherTargetsEvent();
            RaiseLocalEvent(entity, ref targetEvent);
            var tileEntities = targetEvent.Targets;

            // Is there a door on the same grid as the helper? Also checks for windoors and windoorhelpers.
            if (!FindDoor(tileEntities, helperAngle, isWindoorHelper, out var door))
            {
                Log.Warning($"Access Helper ({entity.Owner}) was unable to find boltable door at {coordinates}.");
                QueueDel(entity.Owner);
                return;
            }

            // Does our door have an AccessComponent inside it?
            if (!_accessReaderSystem.GetMainAccessReader(door.Value.Owner, out var accessReader))
            {
                Log.Warning($"Access Helper (Uid {entity.Owner}) was placed on top of an airlock (Uid {door.Value.Owner}) at {Transform(entity.Owner).Coordinates} which has no AccessReader inside it.");
                QueueDel(entity.Owner);
                return;
            }

            // Is the YML for the helper null?
            if (entity.Comp.Access is null)
            {
                Log.Warning($"Access Helper(Uid {entity.Owner}) access is set to null! Try checking the YML file?");
                QueueDel(entity.Owner);
                return;
            }

            if (accessReader.Value.Owner == door.Value.Owner)
            {
                _pending.Add((entity.Owner, door.Value));
                return;
            }

            Log.Warning($"Access Helper (Uid {entity.Owner}) attempted to add access {entity.Comp.Access} to access reader {accessReader} of entity {entity.Owner}");
            _accessReaderSystem.TryAddAccess(accessReader.Value, entity.Comp.Access.Value);
            QueueDel(entity.Owner);
        }

        public override void Update(float frameTime)
        {
            for (var i = _pending.Count - 1; i >= 0; i--)
            {
                var (helperUid, door) = _pending[i];

                if (!TryComp<AccessHelperComponent>(helperUid, out var helperComponent) || helperComponent.Access is null)
                {
                    _pending.RemoveAt(i);
                    continue;
                }

                if (!_accessReaderSystem.GetMainAccessReader(door.Owner, out var accessReader))
                {
                    _pending.RemoveAt(i);
                    continue;
                }

                if (accessReader.Value.Owner == door.Owner)
                    continue;

                _accessReaderSystem.TryAddAccess(accessReader.Value, helperComponent.Access.Value);
                QueueDel(helperUid);
                _pending.RemoveAt(i);
            }
        }


        // Do we have a door on our grid?
        private bool FindDoor(IEnumerable<EntityUid> tileEntities, Angle helperAngle, bool isWindoorHelper, [NotNullWhen(true)] out Entity<AirlockComponent>? door)
        {
            door = null;

            // Starts going through all the entities on the same tile as the helper.
            foreach (var entityUid in tileEntities)
            {
                // Checks if the door has airlockcomponent, returns false if not found
                if (!TryComp<AirlockComponent>(entityUid, out var airlockComp))
                {
                    continue;
                }

                door = (entityUid, airlockComp);
                var isWindoor = _tagSystem.HasTag(door.Value.Owner, TagWindoor);

                // Checks if isWindoorHelper and isWindoor are neither both true nor both false (XOR on two booleans)
                if (isWindoorHelper != isWindoor)
                    continue;

                // If we're a windoor helper, is our door the same angle as us?
                if (isWindoorHelper && !FindDoorAngle(door.Value.Owner, helperAngle))
                    continue;

                return true;
            }

            return false;
        }


        // Find the angle of the door, compare with helperAngle and return true if they're matching.
        private bool FindDoorAngle(EntityUid entityUid, Angle helperAngle)
        {
            var transform = Transform(entityUid);
            var doorAngle = transform.LocalRotation;

            // Is our angle the same as the windoor?
            if (doorAngle != helperAngle)
                return false;

            return true;
        }

    }
}
