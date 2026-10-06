using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Cuffs;
using Content.Shared.Popups;
using Content.Shared.Species.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Species;

/// <summary>
/// Lashes everything adjacent to the user with their tail, then spins them
/// </summary>
public sealed partial class TailwhipSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedCuffableSystem _cuffable = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TailwhipComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TailwhipComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TailwhipComponent, TailwhipEvent>(OnTailwhip);
    }

    private void OnMapInit(Entity<TailwhipComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.ActionPrototype);
    }

    private void OnShutdown(Entity<TailwhipComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    private void OnTailwhip(Entity<TailwhipComponent> ent, ref TailwhipEvent args)
    {
        var cuffed = _cuffable.IsCuffed(ent.Owner);

        if (TryComp<BuckleComponent>(ent, out var buckle) && buckle.Buckled
            || cuffed && TryComp<PullableComponent>(ent, out var pullable) && pullable.BeingPulled)
        {
            _popup.PopupEntity(Loc.GetString("tailwhip-restrained-fail"), ent, ent);
            return;
        }

        if (_stamina.GetStaminaDamage(ent) >= ent.Comp.StaminaToStart)
        {
            _popup.PopupEntity(Loc.GetString("tailwhip-stamina-fail"), ent, ent);
            return;
        }

        var hits = 0;
        var knockedDown = false;

        foreach (var target in _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.Range).ToArray())
        {
            if (target == ent.Owner)
                continue;

            if (!HasComp<DamageableComponent>(target) || !_mobState.IsAlive(target))
                continue;

            if (!_interaction.InRangeUnobstructed(ent.Owner, target, ent.Comp.Range))
                continue;

            _damageable.TryChangeDamage(target, ent.Comp.Damage, origin: ent.Owner);
            _stamina.TakeStaminaDamage(ent.Owner, ent.Comp.StaminaCostPerHit, source: ent.Owner);

            _popup.PopupEntity(
                Loc.GetString("tailwhip-hit-self", ("target", (object)target)),
                Loc.GetString("tailwhip-hit-others", ("user", (object)ent.Owner), ("target", (object)target)),
                target,
                ent.Owner);

            hits++;

            if (cuffed && _random.Prob(ent.Comp.RestrainedKnockdownChance))
            {
                _stun.TryKnockdown(ent.Owner, ent.Comp.RestrainedKnockdownDuration, refresh: true);

                _popup.PopupEntity(
                    Loc.GetString("tailwhip-balance-self"),
                    Loc.GetString("tailwhip-balance-others", ("user", (object)ent.Owner)),
                    ent.Owner,
                    ent.Owner);

                knockedDown = true;
                break;
            }
        }

        if (!knockedDown)
            StartSpin(ent);

        _audio.PlayPredicted(ent.Comp.WhipSound, ent, ent);

        if (knockedDown)
        {
            args.Handled = true;
            return;
        }

        if (hits == 0)
        {
            _popup.PopupEntity(Loc.GetString("tailwhip-no-targets"), ent, ent);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.PopupText), ent, ent);

            if (_stamina.GetStaminaDamage(ent) >= ent.Comp.StaminaToContinue)
                _popup.PopupEntity(Loc.GetString("tailwhip-momentum-fail"), ent, ent);
        }

        args.Handled = true;
    }

    private void StartSpin(Entity<TailwhipComponent> ent)
    {
        ent.Comp.SpinEndTime = _timing.CurTime + ent.Comp.SpinDuration;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TailwhipComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (comp.SpinEndTime is not { } end)
                continue;

            if (_timing.CurTime >= end)
            {
                comp.SpinEndTime = null;
                Dirty(uid, comp);
                continue;
            }

            _transform.SetLocalRotation(uid, xform.LocalRotation + comp.SpinSpeed * frameTime, xform);
        }
    }
}

/// <summary>
/// Raised on the performer when the Tailwhip action is used.
/// </summary>
public sealed partial class TailwhipEvent : InstantActionEvent;
