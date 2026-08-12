using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Flash._Paradise.Components;

namespace Content.Shared.Flash._Paradise;

/// <summary>
/// Modifies eye damage by a given amount.
/// Copied from DamagedByFlashingSystem
/// </summary>
public sealed partial class BlindedByFlashingSystem : EntitySystem
{
    [Dependency] private BlindableSystem _blindable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlindedByFlashingComponent, AfterFlashedEvent>(OnFlashAttempt);
    }

    private void OnFlashAttempt(Entity<BlindedByFlashingComponent> ent, ref AfterFlashedEvent args)
    {
        if (args.Target == ent.Owner)
            _blindable.AdjustEyeDamage(ent.Owner, ent.Comp.EyeDamage);
    }
}
