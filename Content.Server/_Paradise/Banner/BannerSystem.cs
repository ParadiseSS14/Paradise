using Content.Shared._Paradise.Banner;
using Robust.Shared.Timing;

namespace Content.Server._Paradise.Banner;

public sealed partial class BannerSystem : SharedBannerSystem
{
    [Dependency] private IGameTiming _timing = default!;

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

    protected override void Dust(EntityUid uid, BannerComponent comp)
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
