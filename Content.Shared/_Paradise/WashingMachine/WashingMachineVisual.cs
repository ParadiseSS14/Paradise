using Robust.Shared.Serialization;

namespace Content.Shared._Paradise.WashingMachine;

// contains the `AppearanceComponent` keys
[Serializable, NetSerializable]
public enum WashingMachineVisual : byte
{
    State,
    Filled,
}

[Serializable, NetSerializable]
public enum WashingMachineVisualState : byte
{
    Open,
    Closed,
    Running,
}

// Identifys the sprite layer maps.
[Serializable, NetSerializable]
public enum WashingMachineVisualLayers : byte
{
    Door,
    Running,
    Filled,
}
