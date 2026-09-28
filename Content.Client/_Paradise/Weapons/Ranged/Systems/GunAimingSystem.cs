using Content.Client.Weapons.Ranged.Systems;
using Content.Shared._Paradise.Weapons.Components;
using Content.Shared._Paradise.Weapons.Ranged.Events;
using Content.Shared._Paradise.Weapons.Ranged.Systems;
using Content.Shared.CombatMode;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._Paradise.Weapons;

public sealed partial class GunAimingSystem : SharedGunAimingSystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private InputSystem _inputSystem = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private GunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

if (_player.LocalEntity is not EntityUid entity ||
    !TryComp(entity, out CombatModeComponent? combatComp) ||
    !combatComp.IsInCombatMode)
{
    return;
}

        if (!_gun.TryGetGun(entity, out var gun) || !gun.Comp.UseKey)
            return;

        if (!TryComp<GunAimableComponent>(gun.Owner, out var aimableComp))
            return;

        var useKey = EngineKeyFunctions.UseSecondary;

        if (_inputSystem.CmdStates.GetState(useKey) == BoundKeyState.Down && !aimableComp.IsAimed)
        {
            RaisePredictiveEvent(new AimStatusChangeAttemptEvent { Gun = GetNetEntity(gun.Owner), Aim = true, User = GetNetEntity(entity) });
            return;
        }

        if (_inputSystem.CmdStates.GetState(useKey) == BoundKeyState.Up && aimableComp.IsAimed)
            RaisePredictiveEvent(new AimStatusChangeAttemptEvent { Gun = GetNetEntity(gun.Owner), Aim = false, User = GetNetEntity(entity) });
    }
}
