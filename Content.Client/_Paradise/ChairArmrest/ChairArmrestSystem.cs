using System.Numerics;
using Content.Shared._Paradise.ChairArmrest;
using Content.Shared.Buckle.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._Paradise.ChairArmrest;

public sealed partial class ChairArmrestSystem : EntitySystem
{
    [Dependency] private SpriteSystem _spriteSystem = default!;
    [Dependency] private IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChairArmrestComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<ChairArmrestComponent, StrappedEvent>(OnStrap);
        SubscribeLocalEvent<ChairArmrestComponent, UnstrappedEvent>(OnUnstrap);
        SubscribeLocalEvent<ChairArmrestComponent, ComponentShutdown>(OnRemoved);
    }

    private void OnInit(EntityUid uid, ChairArmrestComponent component, ComponentStartup args)
    {
        if (IsClientSide(uid))
            return;

        var localCoords = new EntityCoordinates(uid, Vector2.Zero);
        var overlay = SpawnAttachedTo("ChairArmrestOverlay", localCoords);

        if (!TryComp<SpriteComponent>(overlay, out var sprite))
            return;

        _spriteSystem.LayerSetRsiState((overlay, sprite), 0, new RSI.StateId(component.ArmrestOverlay));
        component.OverlayEntity = overlay;
        sprite.NoRotation = component.ArmrestNoRot;
    }

    private void OnStrap(EntityUid uid, ChairArmrestComponent component, StrappedEvent args)
    {
        SetOverlayVisibility(component, true);
    }

    private void OnUnstrap(EntityUid uid, ChairArmrestComponent component, UnstrappedEvent args)
    {
        SetOverlayVisibility(component, false);
    }

    private void SetOverlayVisibility(ChairArmrestComponent component, bool visible)
    {
        if (!TryComp<SpriteComponent>(component.OverlayEntity, out var sprite) || !_gameTiming.IsFirstTimePredicted)
            return;

        _spriteSystem.SetVisible((component.OverlayEntity.Value, sprite), visible);
    }

    private void OnRemoved(EntityUid uid, ChairArmrestComponent component, ComponentShutdown args)
    {
        QueueDel(component.OverlayEntity);
    }
}
