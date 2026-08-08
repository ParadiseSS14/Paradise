using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Modsuits.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Modsuits.Events;

public sealed partial class DeployModsuit : InstantActionEvent
{
}
public sealed partial class PowerModsuit : InstantActionEvent
{
}
public sealed partial class CheckAirtightnessEvent : EntityEventArgs
{
}
[Serializable, NetSerializable]
public sealed class ModsuitSystemMessage(ModsuitPartType part) : BoundUserInterfaceMessage
{
    public ModsuitPartType Part = part;
}
[Serializable, NetSerializable]
public sealed partial class RetractPartEvent : DoAfterEvent
{
    public NetEntity Performer;
    public NetEntity Modsuit;
    public ModsuitPartType Part;
    public RetractPartEvent(NetEntity performer, NetEntity modsuit, ModsuitPartType part)
    {
        Performer = performer;
        Modsuit = modsuit;
        Part = part;
    }

    public override DoAfterEvent Clone()
    {
        return new RetractPartEvent(Performer, Modsuit, Part);
    }
}
[Serializable, NetSerializable]
public sealed partial class DeployPartEvent : DoAfterEvent
{
    public NetEntity Performer;
    public NetEntity Modsuit;
    public ModsuitPartType Part;
    public DeployPartEvent(NetEntity performer, NetEntity modsuit, ModsuitPartType part)
    {
        Performer = performer;
        Modsuit = modsuit;
        Part = part;
    }

    public override DoAfterEvent Clone()
    {
        return new DeployPartEvent(Performer, Modsuit, Part);
    }
}
