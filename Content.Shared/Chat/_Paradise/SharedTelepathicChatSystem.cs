using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Cloning.Events;

namespace Content.Shared.Chat._Paradise;

public sealed partial class SharedTelepathicChatSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelepathicChatComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<TelepathicChatComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TelepathicChatComponent, CloningEvent>(OnClone);
    }

    private void OnInit(Entity<TelepathicChatComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp(entity, out ActionsComponent? comp))
            return;

        _actions.AddAction(entity, ref entity.Comp.SendActionEntity, entity.Comp.SendAction, component: comp);
        _actions.AddAction(entity, ref entity.Comp.ReceiveActionEntity, entity.Comp.ReceiveAction, component: comp);
    }

    private void OnShutdown(Entity<TelepathicChatComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.SendActionEntity);
        _actions.RemoveAction(entity.Owner, entity.Comp.ReceiveActionEntity);
    }

    private void OnClone(Entity<TelepathicChatComponent> ent, ref CloningEvent args)
    {
        if (!args.Settings.EventComponents.Contains(Factory.GetRegistration(ent.Comp.GetType()).Name))
            return;

        // Make sure to set the datafields before adding the component so that the correct action gets spawned on map init.
        var targetComp = Factory.GetComponent<TelepathicChatComponent>();
        targetComp.SendAction = ent.Comp.SendAction;
        targetComp.ReceiveAction = ent.Comp.ReceiveAction;
        AddComp(args.CloneUid, targetComp, true);
    }
}