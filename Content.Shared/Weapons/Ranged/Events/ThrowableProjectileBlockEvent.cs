using Content.Shared.Damage;

namespace Content.Shared.Weapons.Ranged.Events;


[ByRefEvent]
public record struct ThrowableProjectileBlockAttemptEvent(DamageSpecifier? Damage, EntityUid DamageDealer)
{
    public bool Cancelled = false;

    public DamageSpecifier? Damage = Damage;
}
