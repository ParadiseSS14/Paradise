using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Traits.Assorted;

namespace Content.Client.UserInterface.Systems.DamageOverlays;

public sealed partial class DamageOverlayUiHandlerSystem : EntitySystem
{
    [Dependency] private MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;
    [Dependency] private DamageableSystem _damageable = default!;

    public bool TryGetUpdatedOverlayParameters(EntityUid entity,
        out MobState state,
        out float deadLevel,
        out float critLevel,
        out float oxygenLevel,
        out float painLevel,
        MobStateComponent? mobState,
        DamageableComponent? damageable = null,
        MobThresholdsComponent? thresholds = null,
        InjurableComponent? injurable = null
        )
    {
        state = MobState.Alive;
        deadLevel = 0;
        critLevel = 0;
        oxygenLevel = 0;
        painLevel = 0;
        if (mobState == null && !Resolve(entity, ref mobState) ||
            thresholds == null && !Resolve(entity, ref thresholds) ||
            damageable == null && !Resolve(entity, ref damageable) ||
            injurable == null && !Resolve(entity, ref injurable))
            return false;

        if (!_mobThresholdSystem.TryGetIncapThreshold(entity, out var foundThreshold, thresholds))
            return false; //this entity cannot die or crit!!

        var damagePerGroup = _damageable.GetDamagePerGroup((entity, damageable));
        var critThreshold = foundThreshold.Value;
        state = mobState.CurrentState;

        switch (mobState.CurrentState)
        {
            case MobState.Alive:
                {
                    FixedPoint2 painLevelFP2 = 0;

                    if (!_statusEffects.TryEffectsWithComp<PainNumbnessStatusEffectComponent>(entity, out _))
                    {
                        foreach (var painDamageType in injurable.PainDamageGroups)
                        {

                            damagePerGroup.TryGetValue(painDamageType, out var painDamage);
                            painLevelFP2 += painDamage;
                        }
                        painLevel = FixedPoint2.Min(1f, painLevelFP2 / critThreshold).Float();

                        if (painLevel < 0.05f) // Don't show damage overlay if they're near enough to max.
                            painLevel = 0;
                    }

                    if (damagePerGroup.TryGetValue("Airloss", out var oxyDamage))
                        oxygenLevel = FixedPoint2.Min(1f, oxyDamage / critThreshold).Float();

                    critLevel = 0;
                    deadLevel = 0;
                    break;
                }
            case MobState.Critical:
                {
                    if (!_mobThresholdSystem.TryGetDeadPercentage(entity,
                            FixedPoint2.Max(0.0, _damageable.GetTotalDamage((entity, damageable))), out var critLevelFP2))
                        return false;
                    critLevel = critLevelFP2.Value.Float();

                    painLevel = 0;
                    deadLevel = 0;
                    break;
                }
            case MobState.Dead:
                {
                    painLevel = 0;
                    critLevel = 0;
                    break;
                }
        }

        return true;
    }

}
