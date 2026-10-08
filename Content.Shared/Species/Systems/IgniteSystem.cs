using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IgnitionSource.Components;
using Content.Shared.IgnitionSource.EntitySystems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Species.Components;

namespace Content.Shared.Species;

/// <summary>
/// Lets a species spit a small flame into its own hand
/// </summary>
public sealed partial class IgniteSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;
    [Dependency] private IngestionSystem _ingestion = default!;
    [Dependency] private MatchstickSystem _matchstick = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgniteComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<IgniteComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<IgniteComponent, IgniteEvent>(OnIgnite);
    }

    private void OnMapInit(Entity<IgniteComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.ActionPrototype);
    }

    private void OnShutdown(Entity<IgniteComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    private void OnIgnite(Entity<IgniteComponent> ent, ref IgniteEvent args)
    {
        if (!_ingestion.HasMouthAvailable(ent.Owner))
            return;

        // Have they drunk enough welding fuel? We just care about what is in the stomach and not in the bloodstream
        var stomachs = GetStomachSolutions(ent.Owner);

        var fuel = FixedPoint2.Zero;
        foreach (var stomach in stomachs)
        {
            fuel += stomach.Comp.Solution.GetTotalPrototypeQuantity(ent.Comp.RequiredFuel);
        }

        if (fuel < ent.Comp.RequiredFuelUnits)
        {
            _popup.PopupEntity(Loc.GetString("ignite-no-fuel"), ent, ent);
            return;
        }

        // Is there a free hand to hold the flame?
        if (!_hands.TryGetEmptyHand(ent.Owner, out _))
        {
            _popup.PopupEntity(Loc.GetString("ignite-no-hands"), ent, ent);
            return;
        }

        // Past every failure check, so it is safe to burn the fuel.
        ConsumeStomachFuel(stomachs, ent.Comp.RequiredFuel, ent.Comp.RequiredFuelUnits);

        // Predicted so the player client sees it immediately
        var flame = PredictedSpawnAtPosition(ent.Comp.FlamePrototype, Transform(ent).Coordinates);

        if (TryComp<MatchstickComponent>(flame, out var matchstick))
            _matchstick.TryIgnite((flame, matchstick), ent);

        _hands.TryPickupAnyHand(ent.Owner, flame);

        _popup.PopupEntity(Loc.GetString(ent.Comp.PopupText), ent, ent);

        args.Handled = true;
    }

    /// <summary>
    /// Collects the solution of every stomach in the body.
    /// </summary>
    private List<Entity<SolutionComponent>> GetStomachSolutions(Entity<BodyComponent?> body)
    {
        var solutions = new List<Entity<SolutionComponent>>();

        if (!Resolve(body, ref body.Comp, false))
            return solutions;

        foreach (var organ in body.Comp.Organs?.ContainedEntities ?? [])
        {
            if (!HasComp<StomachComponent>(organ))
                continue;

            if (_solution.TryGetSolution(organ, StomachSystem.DefaultSolutionName, out var solution))
                solutions.Add(solution.Value);
        }

        return solutions;
    }

    /// <summary>
    /// Burns <paramref name="amount"/> of a reagent out of the given stomachs.
    /// </summary>
    private void ConsumeStomachFuel(List<Entity<SolutionComponent>> stomachs, string reagentId, float amount)
    {
        var remaining = FixedPoint2.New(amount);

        foreach (var stomach in stomachs)
        {
            if (remaining <= FixedPoint2.Zero)
                return;

            remaining -= _solution.RemoveReagent(stomach, reagentId, remaining);
        }
    }
}

public sealed partial class IgniteEvent : InstantActionEvent;
