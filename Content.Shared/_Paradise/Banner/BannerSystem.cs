using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Tools.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Paradise.Banner;

public sealed partial class BannerSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BannerComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BannerComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnUseInHand(EntityUid uid, BannerComponent component, UseInHandEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
        {
            _appearanceSystem.SetData(uid, BannerVisuals.State, component.State);
            return;
        }

        // Basically saying set us to the opposite state of whatever we are now.
        component.State = component.State == BannerVisualsState.Rolled ? BannerVisualsState.Unrolled : BannerVisualsState.Rolled;
        args.Handled = true;
    }

    private void OnInteractUsing(EntityUid uid, BannerComponent component, InteractUsingEvent args)
    {
        // Are we an object with the WelderComponent, if so are we enabled?
        if (!TryComp<WelderComponent>(args.Used, out var weldcomp)|| !weldcomp.Enabled)
            return;

        // Are we already on fire?
        if (component.Burning)
            return;


        // Burn baby burn (Set our visual to burning, set our end time 'BurnDuration' seconds from now.)
        component.Burning = true;
        component.BurnEndTime = _timing.CurTime + component.BurnDuration;
        _appearanceSystem.SetData(uid, BannerVisuals.Burning, component.Burning);
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var entities = EntityQueryEnumerator<BannerComponent>();
        while (entities.MoveNext(out var uid, out var comp))
        {
            if (comp.Burning && _timing.CurTime >= comp.BurnEndTime)
            {
                Dust(uid, comp);
            }
        }

    }

    private void Dust(EntityUid uid, BannerComponent comp)
    {
        // Did we get destroyed since we were set on fire?
        if (!Exists(uid))
            return;

        // Let's get our location
        var xform  = Transform(uid);
        var coordinates = xform.Coordinates;

        // Spawn dust where we are and delete us, all done.
        if(_timing.IsFirstTimePredicted)
        {
            comp.Burning = false;
            Spawn("Ash", coordinates);
            QueueDel(uid);
        }
    }

}
