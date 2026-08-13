using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Tools.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Paradise.Banner;

public abstract partial class SharedBannerSystem : EntitySystem
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
        // Basically saying set us to the opposite state of whatever we are now.
        component.State = component.State == BannerVisualsState.Rolled ? BannerVisualsState.Unrolled : BannerVisualsState.Rolled;
        _appearanceSystem.SetData(uid, BannerVisuals.State, component.State);
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
        component.BurnEndTime = _timing.CurTime + TimeSpan.FromSeconds(5);
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

    protected virtual void Dust(EntityUid uid, BannerComponent comp)
    {
    }
}
