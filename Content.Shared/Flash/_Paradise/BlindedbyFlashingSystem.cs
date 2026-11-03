using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Body;
using Content.Shared.Flash._Paradise.Components;

namespace Content.Shared.Flash._Paradise;

/// <summary>
/// Modifies eye damage by a given amount.
/// Copied from DamagedByFlashingSystem
/// </summary>
public sealed partial class BlindedByFlashingSystem : EntitySystem
{
    [Dependency] private BlindableSystem _blindable = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private EntityManager _entman = default!;

    // Flashing is raised on the mob rather than the organ.
    // So these handle applying the component to the mob entity and copying its values.
    [SubscribeLocalEvent]
    private void OnOrganInserted(Entity<BlindedByFlashingComponent> ent, ref OrganGotInsertedEvent args)
    {
        RemComp<BlindedByFlashingComponent>(args.Target);
        _entman.CopyComponent(ent, args.Target, ent.Comp);
    }

    // And when the organ is removed, we remove the added component
    [SubscribeLocalEvent]
    private void OnOrganRemoved(Entity<BlindedByFlashingComponent> ent, ref OrganGotRemovedEvent args)
    {
        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred<BlindedByFlashingComponent>(args.Target);
    }

    [SubscribeLocalEvent]
    private void OnAfterFlashed(Entity<BlindedByFlashingComponent> ent, ref AfterFlashedEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target == ent.Owner)
        {
            _blindable.AdjustEyeDamage(ent.Owner, ent.Comp.EyeDamage);
            args.Handled = true;
        }
    }
}
