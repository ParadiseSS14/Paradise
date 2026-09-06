using Content.Shared._Paradise.Weapons.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;

namespace Content.Shared.Damage.Systems;

public sealed partial class RequireProjectileTargetSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    // PARADISE EDIT START - Add aiming
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private MobStateSystem _state = default!;
    // PARADISE EDIT END

    public override void Initialize()
    {
        SubscribeLocalEvent<RequireProjectileTargetComponent, PreventCollideEvent>(PreventCollide);
        SubscribeLocalEvent<RequireProjectileTargetComponent, StoodEvent>(StandingBulletHit);
        SubscribeLocalEvent<RequireProjectileTargetComponent, DownedEvent>(LayingBulletPass);
    }

    private void PreventCollide(Entity<RequireProjectileTargetComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
          return;

        if (!ent.Comp.Active)
            return;

        var other = args.OtherEntity;
        // PARADISE EDIT START - Add aiming
        if (TryComp(other, out ProjectileComponent? projectile) && (projectile.Shooter is { Valid: true } shooterValidated))
        {
            if (_standing.IsDown(shooterValidated) && //The shooter and the target are both down and the target is alive => we'll hit, otherwise we won't
                _standing.IsDown(ent.Owner) &&
                _state.IsAlive(ent.Owner))
                return;

            if (TryComp<GunAimableComponent>(projectile.Weapon, out var aimComp) && //The shooter is aiming and the target is alive? We hit
                aimComp.IsAimed &&
                _state.IsAlive(ent.Owner))
                return;

            if (CompOrNull<TargetedProjectileComponent>(other)?.Target != ent)
            {
                // Prevents shooting out of while inside of crates
                var shooter = projectile.Shooter;
                if (!shooter.HasValue)
                    return;

                // ProjectileGrenades delete the entity that's shooting the projectile,
                // so it's impossible to check if the entity is in a container
                if (TerminatingOrDeleted(shooter.Value))
                    return;

                if (!_container.IsEntityOrParentInContainer(shooter.Value))
                    args.Cancelled = true;
            }
        }
        // PARADISE EDIT END
    }

    private void SetActive(Entity<RequireProjectileTargetComponent> ent, bool value)
    {
        if (ent.Comp.Active == value)
            return;

        ent.Comp.Active = value;
        Dirty(ent);
    }

    private void StandingBulletHit(Entity<RequireProjectileTargetComponent> ent, ref StoodEvent args)
    {
        SetActive(ent, false);
    }

    private void LayingBulletPass(Entity<RequireProjectileTargetComponent> ent, ref DownedEvent args)
    {
        SetActive(ent, true);
    }
}
