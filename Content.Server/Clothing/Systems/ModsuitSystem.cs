using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Events;
using Content.Server.Temperature.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Modsuits;
using Content.Shared.Modsuits.Components;
using Content.Shared.Modsuits.Events;
using Content.Shared.Toggleable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.Clothing.System;

public sealed partial class ModsuitSystem : SharedModsuitSystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private BarotraumaSystem _barotraumaSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void DeployPart(EntityUid uid, ModsuitComponent component, DeployPartEvent args)
    {
        if (!component.SpawnedParts.TryGetValue(args.Part, out var entity))
            return;
        var perfomer = GetEntity(args.Performer);
        var slot = ModsuitContainers.GetInventorySlot(args.Part);
        if (ModsuitContainers.TryGetStorageContainer(args.Part, out var storageName))
        {
            if (_inventory.TryGetSlotEntity(perfomer, slot, out var oldItem) && oldItem != null)
            {
                if (_inventory.TryUnequip(perfomer, perfomer, slot, force: true))
                {
                    if (_container.TryGetContainer(uid, storageName, out var storage))
                        _container.Insert(oldItem.Value, storage);
                }
            }
        }

        if (!_inventory.TryEquip(perfomer, entity, slot, force: true))
        {
            if (_inventory.TryGetSlotEntity(perfomer, slot, out var oldItem) && oldItem != null)
            {
                _audio.PlayPredicted(component.ErrorSound, uid, perfomer, AudioParams.Default.WithVolume(-2f));
            }
            return;
        }

        _audio.PlayPvs(component.DeploySound, uid, AudioParams.Default.WithVolume(-2f));
        component.DeployedParts[args.Part] = true;
        EnsureComp<UnremoveableComponent>(entity);

        if (!HasComp<UnremoveableComponent>(uid))
        {
            EnsureComp<UnremoveableComponent>(uid);
        }
        RaiseLocalEvent(uid, new CheckAirtightnessEvent());
    }
    protected override void RetractPart(EntityUid uid, ModsuitComponent component, RetractPartEvent args)
    {
        if (!component.SpawnedParts.TryGetValue(args.Part, out var entity))
            return;
        var perfomer = GetEntity(args.Performer);
        RemComp<UnremoveableComponent>(entity);

        var slot = ModsuitContainers.GetInventorySlot(args.Part);

        if (!_inventory.TryUnequip(uid, perfomer, slot, force: true))
            return;

        var container = _container.EnsureContainer<ContainerSlot>(uid, ModsuitContainers.GetPartContainer(args.Part));

        if (!_container.Insert(entity, container))
            return;

        if (ModsuitContainers.TryGetStorageContainer(args.Part, out var storageName))
        {
            if (!_container.TryGetContainer(uid, storageName, out var baseContainer))
                return;
            if (baseContainer is not ContainerSlot storage)
                return;
            if (storage.ContainedEntity != null)
            {
                var oldItem = storage.ContainedEntity.Value;
                _container.Remove(oldItem, storage);
                _inventory.TryEquip(perfomer, oldItem, slot, force: true);
            }
        }
        _audio.PlayPredicted(component.DeploySound, uid, perfomer, AudioParams.Default.WithVolume(-2f));
        component.DeployedParts[args.Part] = false;

        if (!component.DeployedParts.Values.Any(x => x))
            RemComp<UnremoveableComponent>(uid);
        RaiseLocalEvent(uid, new CheckAirtightnessEvent());
    }
    protected override void OnActivate(EntityUid uid, ModsuitComponent component, PowerModsuit args)
    {
        args.Handled = true;
        var delay = 0.0;
        foreach (var partKey in component.SpawnedParts.Keys)
        {
            var currentDelay = delay;
            Timer.Spawn(TimeSpan.FromSeconds(currentDelay), () =>
            {
                var part = component.SpawnedParts[partKey];
                _appearance.SetData(part, ToggleableVisuals.Enabled, component.PowerOn);
                _audio.PlayPvs(component.DeploySound, uid, AudioParams.Default.WithVolume(-2f));
            });
            delay += component.ActivateDelay;
        }
        Timer.Spawn(TimeSpan.FromSeconds(delay), () =>
            {
                _appearance.SetData(uid, ToggleableVisuals.Enabled, component.PowerOn);
                _audio.PlayPvs(component.PowerOnSound, uid, AudioParams.Default.WithVolume(-2f));
            });
        component.PowerOn = !component.PowerOn;
        Dirty(uid, component);
        RaiseLocalEvent(uid, new CheckAirtightnessEvent());
    }
    protected override void CheckAirtightness(Entity<ModsuitComponent> ent, ref CheckAirtightnessEvent args)
    {
        var component = ent.Comp;
        if (component.PowerOn && component.DeployedParts[ModsuitPartType.Helmet] && component.DeployedParts[ModsuitPartType.Chest])
        {
            if (component.ProvidesInternals)
                EnsureComp<BreathToolComponent>(component.SpawnedParts[ModsuitPartType.Helmet]);

            foreach (var part in ModsuitContainers.ProtectionSlots)
            {
                if (_container.TryGetContainingContainer(component.SpawnedParts[part.Key], out var container))
                {
                    var updateEvent = new RefreshPressureProtectionEvent(container.Owner, component.HighPressureMultiplierOnline, 0, component.LowPressureMultiplierOnline, 0);
                    RaiseLocalEvent(component.SpawnedParts[part.Key], updateEvent);
                }
            }
        }
        else
        {
            if (TryComp<BreathToolComponent>(component.SpawnedParts[ModsuitPartType.Helmet], out _))
                RemComp<BreathToolComponent>(component.SpawnedParts[ModsuitPartType.Helmet]);

            foreach (var part in ModsuitContainers.ProtectionSlots)
            {
                if (_container.TryGetContainingContainer(component.SpawnedParts[part.Key], out var container))
                {
                    var updateEvent = new RefreshPressureProtectionEvent(container.Owner, component.HighPressureMultiplierOffline, 0, component.LowPressureMultiplierOffline, 0);
                    RaiseLocalEvent(component.SpawnedParts[part.Key], updateEvent);
                }
            }
        }
    }
}
