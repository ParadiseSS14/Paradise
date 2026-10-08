using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Paradise.StaminaDamageConversion;

public sealed partial class StaminaDamageConversionSystem : EntitySystem
{
    [Dependency] private SharedStaminaSystem _stamina = default!;

    [SubscribeLocalEvent]
    private void OnDamageDealt(Entity<StaminaDamageConversionComponent> ent, ref DamageDealtEvent args)
    {
        if (!args.Damage.AnyPositive())
            return;

        if (TryComp<MobThresholdsComponent>(ent, out var thresholdsComp) && thresholdsComp.CurrentThresholdState == Mobs.MobState.Dead)
            return;

        foreach (var (key, value) in args.Damage.DamageDict)
            if (ent.Comp.ConversionDict.TryGetValue(key, out var conversionValue))
                _stamina.TakeStaminaDamage(ent.Owner, (value * conversionValue).Float());
    }
}
