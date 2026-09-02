using Content.Shared._Paradise.Damageable;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;

namespace Content.Shared._Paradise.Armor;

public sealed partial class DamageTypeRestrictionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageTypeRestrictionComponent, DamageModifyEvent>(OnDamageChange);
    }

    public void OnDamageChange(Entity<DamageTypeRestrictionComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.DamageContainer == null ||
                !ProtoMan.Resolve(ent.Comp.DamageContainer, out var damageContainer))
            return;

        foreach (var type in args.OriginalDamage.DamageDict)
        {
            if (damageContainer.SupportedTypes.Contains(type.Key))
                continue;

            args.Damage.DamageDict[type.Key] = FixedPoint2.Zero;
        }

        args.Damage.ArmourPiercing = args.OriginalDamage.ArmourPiercing;
    }
}
