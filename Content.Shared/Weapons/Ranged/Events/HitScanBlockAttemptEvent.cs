using Content.Shared.Damage;

namespace Content.Shared.Weapons.Ranged.Events;


[ByRefEvent]
public record struct HitscanBlockAttemptEvent(DamageSpecifier? Damage, EntityUid Shooter, bool Cancelled = false)
{
    public Color? hitColor = Color.Red;
}
