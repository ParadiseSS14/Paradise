namespace Content.Shared.Paradise.MindShield;

[ByRefEvent]
public readonly record struct MindshieldProtectionGrantedEvent
{
    public readonly EntityUid Implant;
    public readonly EntityUid Implanted;

    public MindshieldProtectionGrantedEvent(EntityUid implant, EntityUid implanted)
    {
        Implant = implant;
        Implanted = implanted;
    }
}

[ByRefEvent]
public readonly record struct MindshieldProtectionRemovedEvent
{
    public readonly EntityUid Implant;
    public readonly EntityUid Implanted;

    public MindshieldProtectionRemovedEvent(EntityUid implant, EntityUid implanted)
    {
        Implant = implant;
        Implanted = implanted;
    }
}
