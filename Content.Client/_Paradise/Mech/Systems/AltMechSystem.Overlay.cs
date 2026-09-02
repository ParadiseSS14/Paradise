using Content.Client.UserInterface.Systems.DamageOverlays;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Paradise.Mech.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Player;

namespace Content.Client._Paradise.Mech;

public sealed partial class AltMechSystem
{
    [Dependency] private DamageableSystem _damageableSystem = default!;
    [UISystemDependency] private readonly DamageOverlayUiHandlerSystem _handler = default!;

    private void OnPlayerAttach(Entity<AltMechComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        DamageOverlayInit(args.Entity);
    }

    private void DamageOverlayInit(EntityUid entity)
    {
        ClearOverlay();

        if (!TryComp<AltMechComponent>(entity, out var mechComp) || mechComp.PilotSlot.ContainedEntity == null)
            return;

        if (!TryComp<MobStateComponent>(mechComp.PilotSlot.ContainedEntity, out var mobState))
            return;

        _overlay.AddOverlay(_damageOverlay);

        if (mobState.CurrentState != MobState.Dead)
            UpdateDamageOverlays(entity, mobState);
    }

    private void OnPlayerDetached(Entity<AltMechComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlay.RemoveOverlay(_damageOverlay);
        ClearOverlay();
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.Target != _playerManager.LocalEntity)
            return;

        UpdateDamageOverlays(args.Target, args.Component);
    }

    private void OnThresholdCheck(Entity<AltMechPilotComponent> ent, ref MobThresholdChecked args)
    {

        if (!TryComp(args.Target, out AltMechPilotComponent? pilot))
            return;

        if (pilot.Mech != _playerManager.LocalEntity)
            return;

        UpdateDamageOverlays(pilot.Mech, args.MobState, (DamageableComponent?)args.Damageable, args.Threshold);
    }

    private void ClearOverlay()
    {
        _damageOverlay.DeadLevel = 0f;
        _damageOverlay.CritLevel = 0f;
        _damageOverlay.PainLevel = 0f;
        _damageOverlay.OxygenLevel = 0f;
    }

    private void UpdateDamageOverlays(EntityUid entity, MobStateComponent? mobState, DamageableComponent? damageable = null, MobThresholdsComponent? thresholds = null, InjurableComponent? injurable = null)
    {
        if (!TryComp<AltMechComponent>(entity, out var mechComp) || mechComp.PilotSlot.ContainedEntity == null)
            return;

        var pilot = mechComp.PilotSlot.ContainedEntity;

        if (pilot is not { Valid: true } pilotValidated)
            return;

        if (thresholds == null && !TryComp(pilotValidated, out thresholds))
            return;

        if (thresholds != null ||
            TryComp(entity, out thresholds) &&
            !thresholds.ShowOverlays)
        {
            ClearOverlay();
            return;
        }

        _handler.TryGetUpdatedOverlayParameters(
            entity,
            out _damageOverlay.State,
            out _damageOverlay.DeadLevel,
            out _damageOverlay.CritLevel,
            out _damageOverlay.OxygenLevel,
            out _damageOverlay.PainLevel,
            mobState,
            damageable,
            thresholds,
            injurable);
    }
}
