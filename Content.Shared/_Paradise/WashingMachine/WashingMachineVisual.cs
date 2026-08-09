using Robust.Shared.Serialization;

namespace Content.Shared._Paradise.WashingMachine;

[Serializable, NetSerializable]
public enum WashingMachineVisual : byte
{
    State,
    Filled,
    Running,
    Panel,
}

[Serializable, NetSerializable]
public enum WashingMachineVisualState : byte
{
    Open,
    Closed,
}
