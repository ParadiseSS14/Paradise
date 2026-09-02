using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.Paradise.Mech;

[Serializable, NetSerializable]
public enum MechUiKey : byte
{
    Key
}


[Serializable, NetSerializable]
public sealed class MechPartRemoveMessage : BoundUserInterfaceMessage
{
    public PartSlot Part;

    public MechPartRemoveMessage(PartSlot part)
    {
        Part = part;
    }
}

[Serializable, NetSerializable]
public sealed class AltMechEquipmentRemoveMessage : BoundUserInterfaceMessage
{
    public NetEntity Equipment;

    public AltMechEquipmentRemoveMessage(NetEntity equipment)
    {
        Equipment = equipment;
    }
}

[Serializable, NetSerializable]
public sealed class MechMaintenanceToggleMessage : BoundUserInterfaceMessage
{
    public bool Toggled;

    public MechMaintenanceToggleMessage(bool toggled)
    {
        Toggled = toggled;
    }
}

[Serializable, NetSerializable]
public sealed class MechSealMessage : BoundUserInterfaceMessage
{
    public bool Toggled;

    public MechSealMessage(bool toggled)
    {
        Toggled = toggled;
    }
}

[Serializable, NetSerializable]
public sealed class MechBoltMessage : BoundUserInterfaceMessage
{
    public bool Toggled;

    public MechBoltMessage(bool toggled)
    {
        Toggled = toggled;
    }
}

[Serializable, NetSerializable]
public sealed class MechDetachTankMessage : BoundUserInterfaceMessage
{
    public bool Toggled;

    public MechDetachTankMessage(bool toggled)
    {
        Toggled = toggled;
    }
}

[Serializable, NetSerializable]
public sealed class AltMechBoundUiState : BoundUserInterfaceState
{
    public FixedPoint2 TankPressure;

    public FixedPoint2 TankTemperature;
}

public sealed class MechEquipmentUiStateReadyEvent : EntityEventArgs
{
    public Dictionary<NetEntity, BoundUserInterfaceState> States = new();
}

[Serializable, NetSerializable]
public sealed class MechEquipmentRemoveMessage : BoundUserInterfaceMessage
{
    public NetEntity Equipment;

    public MechEquipmentRemoveMessage(NetEntity equipment)
    {
        Equipment = equipment;
    }
}

[Serializable, NetSerializable]
public sealed class MechBoundUiState : BoundUserInterfaceState
{
    public Dictionary<NetEntity, BoundUserInterfaceState> EquipmentStates = new();
}

[Serializable, NetSerializable]
public sealed class MechGrabberUiState : BoundUserInterfaceState
{
    public List<NetEntity> Contents = new();
    public int MaxContents;
}

[Serializable, NetSerializable]
public sealed class MechSoundboardUiState : BoundUserInterfaceState
{
    public List<string> Sounds = new();
}

[Serializable, NetSerializable]
public enum MechVisualLayers : byte
{
    Base
}
