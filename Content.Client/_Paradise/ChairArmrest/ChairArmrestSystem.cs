using System.Linq;
using Content.Shared._Paradise.ChairArmrest;
using Content.Shared.Buckle.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
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
    }

    private void OnInit(EntityUid uid, ChairArmrestComponent component, ComponentStartup args)
    {
        var xform = Transform(uid);
        var coordinates = xform.Coordinates;
        var overlay = Spawn("SofaArmrestOverlay", coordinates);

        if(TryComp<SpriteComponent>(overlay, out SpriteComponent? sprite))
        {
            _spriteSystem.LayerSetRsiState((overlay, sprite), 0, new RSI.StateId(component.ArmrestOverlay));
            component.OverlayEntity = overlay;
        }
    }

    private void OnStrap(EntityUid uid, ChairArmrestComponent component, StrappedEvent args)
    {
        if(TryComp<SpriteComponent>(component.OverlayEntity, out SpriteComponent? sprite) && _gameTiming.IsFirstTimePredicted)
        {
            _spriteSystem.SetVisible((component.OverlayEntity.Value, sprite), true);
        }
    }

    private void OnUnstrap(EntityUid uid, ChairArmrestComponent component, UnstrappedEvent args)
    {
        if(TryComp<SpriteComponent>(component.OverlayEntity, out SpriteComponent? sprite) && _gameTiming.IsFirstTimePredicted)
        {
            _spriteSystem.SetVisible((component.OverlayEntity.Value, sprite), false);
        }
    }
}
