using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Cloning.Events;

namespace Content.Shared.Chat._Paradise;

public sealed partial class SharedTelepathyAbilitySystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelepathyAbilityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<TelepathyAbilityComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TelepathyAbilityComponent, CloningEvent>(OnClone);
    }

    private void OnInit(Entity<TelepathyAbilityComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp(entity, out ActionsComponent? comp))
            return;

        _actions.AddAction(entity, ref entity.Comp.SendActionEntity, entity.Comp.SendAction, component: comp);
        _actions.AddAction(entity, ref entity.Comp.OfferActionEntity, entity.Comp.OfferAction, component: comp);
    }

    private void OnShutdown(Entity<TelepathyAbilityComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.SendActionEntity);
        _actions.RemoveAction(entity.Owner, entity.Comp.OfferActionEntity);
    }

    private void OnClone(Entity<TelepathyAbilityComponent> ent, ref CloningEvent args)
    {
        if (!args.Settings.EventComponents.Contains(Factory.GetRegistration(ent.Comp.GetType()).Name))
            return;

        // Make sure to set the datafields before adding the component so that the correct action gets spawned on map init.
        var targetComp = Factory.GetComponent<TelepathyAbilityComponent>();
        targetComp.SendAction = ent.Comp.SendAction;
        targetComp.OfferAction = ent.Comp.OfferAction;
        AddComp(args.CloneUid, targetComp, true);
    }
}