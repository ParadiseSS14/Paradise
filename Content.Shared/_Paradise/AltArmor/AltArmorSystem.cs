using Content.Shared._Paradise.AltArmor.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.AltArmor;

public abstract partial class AltArmorSystem<T> : EntitySystem where T : AltArmorComponent
{
    [Dependency] private DamageableSystem _damageable = default!;

    public void ModifyDamage(Entity<T?> ent, DamageSpecifier? damage, out DamageSpecifier resultDamage, out DamageSpecifier resultArmorDamage)
    {
        resultDamage = new DamageSpecifier();
        resultArmorDamage = new DamageSpecifier();

        if (damage == null)
            return;

        if (!Resolve(ent.Owner, ref ent.Comp))
        {
            resultDamage = damage;
            return;
        }

        FixedPoint2 maximalDamage = 0;
        string? maximalDamageType = null;

        FixedPoint2 durabilityCoefficient = 1;

        if (TryComp<DamageableComponent>(ent, out var damageableComp) && ent.Comp.DamageAffectsProtection)
        {
            int zeroThreshold = ent.Comp.ZeroProtectionThreshold;

            if (zeroThreshold <= 0)
                zeroThreshold = 100; //default value

            durabilityCoefficient = 1 - _damageable.GetTotalDamage(ent.Owner) / zeroThreshold;

            durabilityCoefficient = FixedPoint2.Clamp(durabilityCoefficient, 0, 1);
        }

        foreach (var type in damage.DamageDict.Keys)//Here we start counting damage for each type
        {
            if (ent.Comp.SelfDamageReductionByType.ContainsKey(type))
                CountDifference(
                    resultArmorDamage.DamageDict,
                    damage.DamageDict[type],
                    ent.Comp.SelfDamageReductionByType[type],
                    type,
                    piercing: damage.ArmorPenetration,
                    durabilityCoefficient: durabilityCoefficient
                );//armor damage
            else
                resultArmorDamage.DamageDict.Add(type, damage.DamageDict[type]);

            if (ent.Comp.UserDamageReductionByType.ContainsKey(type))
            {
                var damageDiff = CountDifference(
                    resultDamage.DamageDict,
                    damage.DamageDict[type],
                    ent.Comp.UserDamageReductionByType[type],
                    type,
                    damage.ArmorPenetration,
                    durabilityCoefficient: durabilityCoefficient
                );//user damage

                if (damageDiff > maximalDamage)
                {
                    maximalDamage = damageDiff;
                    maximalDamageType = type;
                }

                if (ent.Comp.TransformSpecifierDict.ContainsKey(type) && ent.Comp.UserDamageReductionByType.ContainsKey(ent.Comp.TransformSpecifierDict[type]))
                    CountDifference(
                        resultDamage.DamageDict,
                        damage.DamageDict[type] - damageDiff,
                        ent.Comp.UserDamageReductionByType[ent.Comp.TransformSpecifierDict[type]],
                        ent.Comp.TransformSpecifierDict[type], FixedPoint2.Zero,
                        durabilityCoefficient: durabilityCoefficient
                    ); //Piercing is not applied here

                continue;

            }

            CountDifference(resultDamage.DamageDict, damage.DamageDict[type], FixedPoint2.Zero, type, FixedPoint2.Zero, durabilityCoefficient: durabilityCoefficient);
        }

        if (maximalDamageType != null)
        {
            if (damage.ArmorPenetration > ent.Comp.UserDamageReductionByType[maximalDamageType])
            {
                resultDamage.ArmorPenetration = damage.ArmorPenetration - ent.Comp.UserDamageReductionByType[maximalDamageType];
                return;
            }
            resultDamage.ArmorPenetration = 0;
        }
    }

    public FixedPoint2 CountDifference(Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> dict, FixedPoint2 damage, FixedPoint2 resist, ProtoId<DamageTypePrototype> type, FixedPoint2 piercing, FixedPoint2 durabilityCoefficient)
    {
        resist *= durabilityCoefficient;
        resist = FixedPoint2.Max(resist - piercing, FixedPoint2.Zero);

        if (damage > resist)
        {
            if (dict.ContainsKey(type))
            {
                dict[type] += damage - resist;
                return damage - resist;
            }

            dict.Add(type, damage - resist);
            return damage - resist;
        }
        return 0;
    }
}
