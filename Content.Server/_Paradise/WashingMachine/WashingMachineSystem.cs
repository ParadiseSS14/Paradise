using Content.Server.Power.Components;
using Content.Shared._Paradise.WashingMachine;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Paradise.WashingMachine;

public sealed partial class WashingMachineSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private SharedAudioSystem _audioSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private SharedItemSystem _itemSystem = default!;

    public const string WashingmachineContainer = "washingmachine_storage";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WashingMachineComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<WashingMachineComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<WashingMachineComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<WashingMachineComponent, GetVerbsEvent<AlternativeVerb>>(AddVerbs);
        SubscribeLocalEvent<WashingMachineComponent, PowerChangedEvent>(OnPowerChange);
        SubscribeLocalEvent<WashingMachineComponent, EntInsertedIntoContainerMessage>(ContentUpdate);
        SubscribeLocalEvent<WashingMachineComponent, EntRemovedFromContainerMessage>(ContentUpdate);
    }

    private void OnCompInit(Entity<WashingMachineComponent> entity, ref ComponentInit args)
    {
        entity.Comp.Storage = _container.EnsureContainer<Container>(entity.Owner, WashingmachineContainer);
    }

    private void OnInteractHand(Entity<WashingMachineComponent> entity, ref InteractHandEvent args)
    {
        if (entity.Comp.State == WashingMachineVisualState.Running)
            return;
        var doorstate = entity.Comp.State == WashingMachineVisualState.Closed
            ? WashingMachineVisualState.Open
            : WashingMachineVisualState.Closed;
        SetState(entity, doorstate);
        _audioSystem.PlayPvs(entity.Comp.DoorSound, entity.Owner, AudioParams.Default.WithVolume(-5f).WithMaxDistance(2f));
        args.Handled = true;
    }

    private void OnInteractUsing(Entity<WashingMachineComponent> entity, ref InteractUsingEvent args)
    {
        if (entity.Comp.State == WashingMachineVisualState.Running || entity.Comp.State == WashingMachineVisualState.Closed)
            return;
        if (entity.Comp.Storage.Count >= entity.Comp.MaxItemCapacity)
        {
            _popupSystem.PopupEntity(Loc.GetString("washing-container-full"), entity.Owner, args.User);
            return;
        }
        if (!TryComp<ItemComponent>(args.Used, out var itemComp) || _itemSystem.GetItemSizeWeight(itemComp.Size) >= _itemSystem.GetItemSizeWeight(entity.Comp.MaxItemSize))
        {
            _popupSystem.PopupEntity(Loc.GetString("washing-item-oversized"), entity.Owner, args.User);
            return;
        }
        _container.Insert(args.Used, entity.Comp.Storage);
        args.Handled = true;
    }

    private void AddVerbs(Entity<WashingMachineComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (entity.Comp.State == WashingMachineVisualState.Closed && entity.Comp.Storage.Count > 0)
        {
            AlternativeVerb startwashverb = new()
            {
                Text = Loc.GetString("washing-cyle-start"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/clock.svg.192dpi.png")),
                Act = () =>
                {
                    StartWash(entity);
                },
            };
            args.Verbs.Add(startwashverb);
        }
        // If we are open, show emptycontentverb.
        if(entity.Comp.State == WashingMachineVisualState.Open)
        {
            AlternativeVerb emptycontentsverb = new()
            {
                Text = Loc.GetString("washing-empty-container"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/eject.svg.192dpi.png")),
                Act = () =>
                {
                    _container.EmptyContainer(entity.Comp.Storage);
                },
            };
            args.Verbs.Add(emptycontentsverb);
        }
    }

    private void OnPowerChange(Entity<WashingMachineComponent> entity, ref PowerChangedEvent args)
    {
        if(entity.Comp.State !=  WashingMachineVisualState.Open)
            SetState(entity, WashingMachineVisualState.Closed);
        entity.Comp.PlayingStream = _audioSystem.Stop(entity.Comp.PlayingStream);
    }

    private void ContentUpdate(EntityUid uid, WashingMachineComponent component, ContainerModifiedMessage args)
    {
        if (component.Storage != args.Container)
            return;
        _appearanceSystem.SetData(uid, WashingMachineVisual.Filled, component.Storage.Count > 0);
    }

    private void StartWash(Entity<WashingMachineComponent> entity)
    {
        if (!(TryComp<ApcPowerReceiverComponent>(entity.Owner, out var apc) && apc.Powered))
            return;

        entity.Comp.PlayingStream = _audioSystem.PlayPvs(entity.Comp.RunningSound, entity.Owner, AudioParams.Default.WithLoop(true).WithMaxDistance(2f))?.Entity;
        entity.Comp.WashEndTime = _timing.CurTime + entity.Comp.WashDuration;
        SetState(entity, WashingMachineVisualState.Running);
    }

    private void SetState(Entity<WashingMachineComponent> entity, WashingMachineVisualState state)
    {
        if (entity.Comp.State == state)
            return;
        entity.Comp.State = state;
        _appearanceSystem.SetData(
            entity.Owner,
            WashingMachineVisual.State,
            state);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WashingMachineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.State != WashingMachineVisualState.Running)
                continue;

            if (_timing.CurTime <= comp.WashEndTime)
                continue;
            // Ideally there'd be some code in here to wash the items inside, but we don't HAVE blood staining mechanics!! (yet)
            SetState((uid, comp), WashingMachineVisualState.Closed);
           comp.PlayingStream = _audioSystem.Stop(comp.PlayingStream);
            _audioSystem.PlayPvs(comp.FinishSound, uid, AudioParams.Default.WithVolume(-5f).WithMaxDistance(2f));
        }
    }

}
