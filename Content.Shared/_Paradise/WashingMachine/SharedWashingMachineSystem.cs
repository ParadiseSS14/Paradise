using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._Paradise.WashingMachine;

public abstract partial class SharedWashingMachineSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private SharedAudioSystem _audioSystem = default!;
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

    }

    private void OnCompInit(Entity<WashingMachineComponent> entity, ref ComponentInit args)
    {
        // Gives our washing machine a container.
        entity.Comp.Storage = _container.EnsureContainer<Container>(entity.Owner, WashingmachineContainer);
    }

    private void OnInteractHand(Entity<WashingMachineComponent> entity, ref InteractHandEvent args)
    {
        // If we're running, don't do anything.
        if (entity.Comp.IsRunning)
            return;

        // If our state is closed, set it to open and vice versa.
        var doorstate = entity.Comp.State == WashingMachineVisualState.Closed
            ? WashingMachineVisualState.Open
            : WashingMachineVisualState.Closed;

        // Update our appearance to our new state and play a sound.
        SetState(entity, doorstate);
        _audioSystem.PlayPvs(entity.Comp.DoorSound, entity.Owner, AudioParams.Default.WithVolume(-5f).WithMaxDistance(2f));
        args.Handled = true;
    }

    private void OnInteractUsing(Entity<WashingMachineComponent> entity, ref InteractUsingEvent args)
    {
        // If we're running or our doors closed, return.
        if (entity.Comp.IsRunning || entity.Comp.State == WashingMachineVisualState.Closed)
            return;

        // If we're already full, return and popup message
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

        // Put the used item into our storage, then change our sprite.
        _container.Insert(args.Used, entity.Comp.Storage);
        _appearanceSystem.SetData(entity.Owner, WashingMachineVisual.Filled, true);
        args.Handled = true;
    }

    private void AddVerbs(Entity<WashingMachineComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        // If we're not running, not open, nor empty, show startwashverb.
        if (!entity.Comp.IsRunning && entity.Comp.State == WashingMachineVisualState.Closed && entity.Comp.Storage.Count >= 0)
        {
            AlternativeVerb startwashverb = new()
            {
                Text = Loc.GetString("washing-cyle-start"),
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
                Text = Loc.GetString("washing-cyle-emptycontents"),
                Act = () =>
                {
                    _container.EmptyContainer(entity.Comp.Storage);
                    _appearanceSystem.SetData(entity.Owner, WashingMachineVisual.Filled, false);
                },
            };
            args.Verbs.Add(emptycontentsverb);
        }

    }


    // Start our wash cycle, handled in server-side WashingMachineSystem
    protected virtual void StartWash(Entity<WashingMachineComponent> entity)
    {
    }

    protected virtual void SetState(Entity<WashingMachineComponent> entity, WashingMachineVisualState state)
    {
    }
}
