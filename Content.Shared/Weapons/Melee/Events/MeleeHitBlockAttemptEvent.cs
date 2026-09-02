namespace Content.Shared.Weapons.Melee.Events;


[ByRefEvent]
public record struct MeleeHitBlockAttemptEvent(EntityUid Attacker, bool Cancelled = false)
{
    public EntityUid Blocker;

    public Color? HitMarkColor = Color.Red;
}
