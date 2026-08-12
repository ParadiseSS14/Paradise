using Content.Shared.Item;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.WashingMachine;

[RegisterComponent]
public sealed partial class WashingMachineComponent : Component
{
    // Where our items are held
    public Container Storage = default!;

    // State of our door sprite (Open/Closed)
    [DataField]
    public WashingMachineVisualState State = WashingMachineVisualState.Closed;

    // Are we currently running
    [DataField]
    public bool IsRunning = false;

    // Sound for opening our door
    [DataField]
    public SoundSpecifier DoorSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    // Sound while it is running
    [DataField]
    public SoundSpecifier RunningSound = new SoundPathSpecifier("/Audio/Machines/_Paradise/washing_machine_running.ogg");

    // Sound for finishing our wash cycle
    [DataField]
    public SoundSpecifier FinishSound = new SoundPathSpecifier("/Audio/Machines/ding.ogg");

    // Maximum amount of items allowed in washer
    [DataField]
    public int MaxItemCapacity = 3;

    // Maximum size of item allowed in washer
    [DataField]
    public ProtoId<ItemSizePrototype> MaxItemSize = "Large";

    // Duration of our wash cycle
    [DataField]
    public TimeSpan WashDuration = TimeSpan.FromSeconds(6);

    // Time when our cycle ends
    public TimeSpan WashEndTime;

    // Current playing looping audio
    public EntityUid? PlayingStream;
}
