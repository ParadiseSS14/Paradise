using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class HitscanBlockSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanBasicDamageComponent, AttemptHitscanRaycastFiredEvent>(OnHitscanHit);
    }

    private void OnHitscanHit(Entity<HitscanBasicDamageComponent> ent, ref AttemptHitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity == null || args.Data.Shooter == null)
            return;

        var ev = new HitscanBlockAttemptEvent(ent.Comp.Damage, (EntityUid)args.Data.Shooter);

        RaiseLocalEvent((EntityUid)args.Data.HitEntity, ref ev);

        args.Cancelled = ev.Cancelled;
    }

}
