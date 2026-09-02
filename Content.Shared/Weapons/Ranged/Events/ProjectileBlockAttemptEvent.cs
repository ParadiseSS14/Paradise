using Content.Shared.Damage;

namespace Content.Shared.Weapons.Ranged.Events;

[ByRefEvent]
public record struct ProjectileBlockAttemptEvent(EntityUid ProjUid, DamageSpecifier Damage, bool Cancelled = false)
{
    public Color? hitMarkColor = Color.Red;
}
