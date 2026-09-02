namespace Content.Shared.Weapons.Melee.Events;

using Content.Shared.Damage;

[ByRefEvent]
public record struct MeleeAttackerEvent(EntityUid used, EntityUid target, DamageSpecifier damage)
{
    public EntityUid Used = used;
    public EntityUid Target = target;
    public DamageSpecifier Damage = damage;
    public DamageSpecifier ModifiedDamage = new DamageSpecifier();
}
