using Content.Shared.Actions;
using Content.Shared.Modsuits.Components;
using Content.Shared.Modsuits.Events;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Content.Shared.DoAfter;
namespace Content.Shared.Modsuits;

/// <summary>
/// System for handling modsuit actions and events, such as equipping, unequipping, and deploying the modsuit.
/// This system controls any actions with the modsuit.
/// </summary>
public abstract partial class SharedModsuitSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModsuitComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<ModsuitComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ModsuitComponent, GotUnequippedEvent>(OnUnequipped);
        /// events for UI handle
        SubscribeLocalEvent<ModsuitComponent, DeployModsuit>(OpenRadialUI);
        SubscribeLocalEvent<ModsuitComponent, ModsuitSystemMessage>(OnSystemMessage);
        SubscribeLocalEvent<ModsuitComponent, PowerModsuit>(OnActivate);
        // events for modsuit state change
        SubscribeLocalEvent<ModsuitComponent, DeployPartEvent>(DeployPart);
        SubscribeLocalEvent<ModsuitComponent, RetractPartEvent>(RetractPart);
        SubscribeLocalEvent<ModsuitComponent, CheckAirtightnessEvent>(CheckAirtightness);
    }

    /// <summary>
    /// Spawns the modsuit parts into their respective containers when the modsuit is initialized on the map.
    /// </summary>
    private void OnMapInit(EntityUid uid, ModsuitComponent component, MapInitEvent args)
    {
        foreach (var (part, prototype) in component.Parts)
        {
            var container = _container.EnsureContainer<ContainerSlot>(
                uid,
                ModsuitContainers.GetPartContainer(part));

            if (container.ContainedEntity != null)
            {
                component.SpawnedParts[part] = container.ContainedEntity.Value;
                component.DeployedParts[part] = false;
                continue;
            }

            var entity = Spawn(prototype.ToString(), Transform(uid).Coordinates);

            if (!_container.Insert(entity, container))
            {
                Del(entity);
                continue;
            }

            component.SpawnedParts.TryAdd(part, entity);
            component.DeployedParts.TryAdd(part, false);
            Dirty(uid, component);
        }
    }
    /// <summary>
    /// Give deploy, power on and status panel access actions when the modsuit is equipped
    /// </summary>
    private void OnEquipped(EntityUid uid, ModsuitComponent component, GotEquippedEvent args)
    {
        foreach (var action in component.ActionEndpoints)
        {
            EntityUid? entity = null;
            if (_actions.AddAction(args.EquipTarget, ref entity, out _, action, uid))
                component.ActionEntities.Add(entity.Value);
            Dirty(uid, component);
        }
    }
    /// <summary>
    /// Remove deploy, power on and status panel access actions when the modsuit is unequipped
    /// </summary>
    private void OnUnequipped(EntityUid uid, ModsuitComponent component, GotUnequippedEvent args)
    {
        foreach (var action in component.ActionEntities)
        {
            _actions.RemoveAction(args.EquipTarget, action);
            Dirty(uid, component);
        }

        component.ActionEntities.Clear();
    }
    protected virtual void OnActivate(EntityUid uid, ModsuitComponent component, PowerModsuit args)
    {
    }
    private void OpenRadialUI(EntityUid uid, ModsuitComponent component, DeployModsuit args)
    {
        args.Handled = true;
        _ui.OpenUi(uid, ModsuitUiKey.Radial, args.Performer);
    }
    private void OnSystemMessage(EntityUid uid, ModsuitComponent component, ModsuitSystemMessage args)
    {
        DoAfterEvent doAfter;
        if (component.DeployedParts.TryGetValue(args.Part, out var deployed) && deployed)
        {
            doAfter = new RetractPartEvent(GetNetEntity(args.Actor), GetNetEntity(uid), args.Part);
        }
        else
            doAfter = new DeployPartEvent(GetNetEntity(args.Actor), GetNetEntity(uid), args.Part);
        var doAfterArgs = new DoAfterArgs(EntityManager, args.Actor, TimeSpan.FromSeconds(component.PowerOn ? component.ActivateDelay : 0), doAfter, uid);
        _doAfter.TryStartDoAfter(doAfterArgs);
    }
    protected virtual void DeployPart(EntityUid uid, ModsuitComponent component, DeployPartEvent args)
    {
    }
    protected virtual void RetractPart(EntityUid uid, ModsuitComponent component, RetractPartEvent args)
    {
    }
    protected virtual void CheckAirtightness(Entity<ModsuitComponent> ent, ref CheckAirtightnessEvent args)
    {
    }
}
