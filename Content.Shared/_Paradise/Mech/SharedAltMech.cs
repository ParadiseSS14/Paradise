using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Paradise.Mech;

[ByRefEvent]
public readonly record struct MechPartInsertedEvent(EntityUid Mech)
{
    public readonly EntityUid Mech = Mech;
}

[Serializable, NetSerializable]
public sealed partial class MechPartInsertedDoAfterEvent : SimpleDoAfterEvent
{

}

[ByRefEvent]
public readonly record struct MechPartRemovedEvent(EntityUid Mech)
{
    public readonly EntityUid Mech = Mech;
}

[ByRefEvent]
public readonly record struct MechSpeedModifiedEvent(EntityUid Mech)
{
    public readonly EntityUid Mech = Mech;
}

[ByRefEvent]
public readonly record struct OnMechExitEvent();

[ByRefEvent]
public readonly record struct OnMechEntryEvent();

[ByRefEvent]
public readonly record struct MassChangedEvent();

public enum PartSlot : byte
{
    Core = 0,
    Head = 1,
    RightArm = 2,
    LeftArm = 3,
    Chassis = 4,
    Power = 5
}

[Serializable, NetSerializable]
public sealed partial class InsertPartEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class MechBoltsSawedEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class InsertEquipmentEvent : SimpleDoAfterEvent
{
}

[ByRefEvent]
public readonly record struct MechEquipmentInsertedEvent(EntityUid Mech)
{
    public readonly EntityUid Mech = Mech;
}

[ByRefEvent]
public readonly record struct MechEquipmentRemovedEvent(EntityUid Mech)
{
    public readonly EntityUid Mech = Mech;
}

[ByRefEvent]
public record struct RefreshOpticHudEvent<T>() where T : IComponent
{
    public bool Active = false;
    public List<T> Components = new();
}

public sealed partial class MechRelayActionEvent : InstantActionEvent
{
    public EntityUid ActionToPerform;
    public EntityUid ActionUser;
}

[Serializable, NetSerializable]
public sealed partial class RemoveBatteryEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class MechExitEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class MechEntryEvent : SimpleDoAfterEvent
{
}

public sealed partial class MechOpenUiEvent : InstantActionEvent
{
}

public sealed partial class MechEjectPilotEvent : InstantActionEvent
{
}

public enum MechPartVisualLayers : byte
{
    Core = 0,
    CoreColored = 1,
    Head = 2,
    HeadColored = 3,
    Chassis = 4,
    ChassisColored = 5,
    RightArm = 6,
    RightArmColored = 7,
    LeftArm = 8,
    LeftArmColored = 9,
    Power = 10,
    PowerColored = 11
}

public enum MechCameraVisualLayer : byte
{
    Camera = 0
}
