using Content.Shared.Item;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.WashingMachine;

[RegisterComponent]
public sealed partial class WashingMachineComponent : Component
{
    // Where our items are helpd
    public Container Storage = default!;

    // State of our door sprite (Open/Closed)
    [DataField]
    public WashingMachineVisualState State = WashingMachineVisualState.Closed;

    // Whether we have items in us or not
    [DataField]
    public bool Filled = false;

    // If our maintenance panel is open (Not currently used)
    [DataField]
    public bool Panel = false;

    // Are we currently running
    [DataField]
    public bool Running = false;

    // Sound for opening our door
    [DataField]
    public SoundSpecifier DoorSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    // Sound for finishing our wash cycle
    [DataField]
    public SoundSpecifier FinishSound = new SoundPathSpecifier("/Audio/Machines/ding.ogg");

    // Time when our cycle ends
    [DataField]
    public TimeSpan WashEndTime;

    // Maximum amount of items allowed in washer
    [DataField]
    public int MaxItems = 3;

    // Maximum size of item allowed in washer
    [DataField]
    public ProtoId<ItemSizePrototype> MaxItemSize = "Large";

}
