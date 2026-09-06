using Content.Client.Weapons.Ranged.Systems;
using Content.Shared._Paradise.Weapons.Components;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._Paradise.Weapons;

public sealed partial class GunTargetingOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private IEntityManager _entManager;
    private IEyeManager _eye;
    private IGameTiming _timing;
    private IInputManager _input;
    private IPlayerManager _player;
    private GunSystem _guns;
    private SharedTransformSystem _transform;

    public GunTargetingOverlay(IEntityManager entManager, IEyeManager eyeManager, IGameTiming timing, IInputManager input, IPlayerManager player, GunSystem system, SharedTransformSystem transform)
    {
        _entManager = entManager;
        _eye = eyeManager;
        _input = input;
        _timing = timing;
        _player = player;
        _guns = system;
        _transform = transform;
    }

    private readonly Color _defaultOverlayColor = Color.LightBlue;

    private readonly Color _activeOverlayColor = Color.Orange;

    protected override void Draw(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;

        if (_player.LocalEntity is not {Valid: true} playerValid ||
            !_entManager.TryGetComponent<TransformComponent>(playerValid, out var xform))
            return;

        var mapPos = _transform.GetMapCoordinates(playerValid, xform: xform);

        if (mapPos.MapId == MapId.Nullspace)
            return;

        if (!_guns.TryGetGun(playerValid, out var gun))
            return;

        var mouseScreenPos = _input.MouseScreenPosition;
        var mousePos = _eye.PixelToMap(mouseScreenPos);

        if (mapPos.MapId != mousePos.MapId)
            return;

        // (☞ﾟヮﾟ)☞
        var maxSpread = gun.Comp.MaxAngleModified;
        var minSpread = gun.Comp.MinAngleModified;
        var timeSinceLastFire = (_timing.CurTime - gun.Comp.NextFire).TotalSeconds;
        var currentAngle = new Angle(MathHelper.Clamp(gun.Comp.CurrentAngle.Theta - gun.Comp.AngleDecayModified.Theta * timeSinceLastFire,
            gun.Comp.MinAngleModified.Theta, gun.Comp.MaxAngleModified.Theta));
        var direction = (mousePos.Position - mapPos.Position);

        var overlayColor = _defaultOverlayColor;

        if (_entManager.TryGetComponent<GunAimableComponent>(gun.Owner, out var aimableComp) && aimableComp.IsAimed)
            overlayColor = _activeOverlayColor;

        // Show current angle
        worldHandle.DrawCircle(mapPos.Position + currentAngle.RotateVec(direction), 0.08f, overlayColor, true);
        worldHandle.DrawCircle(mapPos.Position + (-currentAngle).RotateVec(direction), 0.08f, overlayColor, true);
    }
}
