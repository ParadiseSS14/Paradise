using Content.Shared._Paradise.CustomColorableLayer;
using Robust.Client.GameObjects;

namespace Content.Client._Paradise.CustomColorableLayer;

public sealed partial class CustomColorableLayerSystem : SharedCustomColorableLayerSystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomColorableLayerComponent, ComponentStartup>(OnComponentStartup);
    }

    protected override void OnPaintDoAfter(Entity<CustomColorableLayerComponent> ent, ref CustomColorPaintEvent args)
    {
        base.OnPaintDoAfter(ent, ref args);

        if (TryComp<SpriteComponent>(ent.Owner, out var spriteComp))
            _sprite.LayerSetColor((ent, spriteComp), ent.Comp.AttachedColoredSpriteLayer, ent.Comp.ColoredLayerColor);
    }

    public void OnComponentStartup(Entity<CustomColorableLayerComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(ent.Owner, out var spriteComp))
            _sprite.LayerSetColor((ent, spriteComp), ent.Comp.AttachedColoredSpriteLayer, ent.Comp.ColoredLayerColor);
    }
}
