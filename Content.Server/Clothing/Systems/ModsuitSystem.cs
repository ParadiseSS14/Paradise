using System.Linq;
using System.Runtime.CompilerServices;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Events;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Events;
using Content.Server.Temperature.Systems;
using Content.Shared.Atmos.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Modsuits;
using Content.Shared.Modsuits.Components;
using Content.Shared.Modsuits.Events;
using Content.Shared.Toggleable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server.Clothing.System;

public sealed partial class ModsuitSystem : SharedModsuitSystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private BarotraumaSystem _barotraumaSystem = default!;
    [Dependency] private TemperatureSystem _temperatureSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PressureProtectionComponent, RefreshPressureProtectiondModifiersEvent>(OnPressureProtectionEvent);
        SubscribeLocalEvent<TemperatureProtectionComponent, RefreshTempratureProtectiondModifiersEvent>(OnTempratureProtectionEvent);
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
        if (component.PowerOn)
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
        if (component.PowerOn)
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
        var helmet = component.SpawnedParts[ModsuitPartType.Helmet];
        if (component.PowerOn && component.DeployedParts[ModsuitPartType.Helmet] && component.DeployedParts[ModsuitPartType.Chest])
        {
            EnsureComp<BreathToolComponent>(helmet);
            EnsureComp<IdentityBlockerComponent>(helmet);
            if (!EnsureComp<HideLayerClothingComponent>(helmet, out var hideComp))
            {
                hideComp.Layers = new()
                {
                    {HumanoidVisualLayers.Hair, SlotFlags.HEAD},
                    {HumanoidVisualLayers.Snout, SlotFlags.HEAD},
                    {HumanoidVisualLayers.HeadTop, SlotFlags.HEAD},
                    {HumanoidVisualLayers.HeadSide, SlotFlags.HEAD},
                    {HumanoidVisualLayers.FacialHair, SlotFlags.HEAD},
                };
            }
            if (TryComp<ClothingComponent>(helmet, out var clothComp) && _container.TryGetContainingContainer(helmet, out var container))
            {
                var clothingGotEquippedEvent = new ClothingGotEquippedEvent(container.Owner, clothComp);
                RaiseLocalEvent(helmet, ref clothingGotEquippedEvent);
            }
            foreach (var part in ModsuitContainers.ProtectionSlots)
            {
                var partEntity = component.SpawnedParts[part.Key];
                if (component.ProvidesPressureProtection)
                {
                    if (TryComp<PressureProtectionComponent>(partEntity, out var pressureProtection))
                        _barotraumaSystem.RefresPressureProtectionModifiers((partEntity, pressureProtection));
                }
                if (component.ProvidesTempretureProtection)
                {
                    if (TryComp<TemperatureProtectionComponent>(partEntity, out var temperatureProtection))
                        _temperatureSystem.RefresTempratureProtectionModifiers((partEntity, temperatureProtection));
                }
                if (TryComp<ModsuitClothingComponent>(partEntity, out var modComp))
                    modComp.ValuesChanged = true;
            }
        }
        else
        {
            if (!component.DeployedParts[ModsuitPartType.Helmet])
            {
                if (TryComp<ClothingComponent>(helmet, out var clothComp) && _container.TryGetContainingContainer(helmet, out var container))
                {
                    var clothingGotUnequippedEvent = new ClothingGotUnequippedEvent(container.Owner, clothComp);
                    RaiseLocalEvent(helmet, ref clothingGotUnequippedEvent);
                }
                RemComp<BreathToolComponent>(helmet);
                RemComp<IdentityBlockerComponent>(helmet);
                RemComp<HideLayerClothingComponent>(helmet);
            }
            foreach (var part in ModsuitContainers.ProtectionSlots)
            {
                var partEntity = component.SpawnedParts[part.Key];
                if (TryComp<PressureProtectionComponent>(partEntity, out var pressureProtection))
                    _barotraumaSystem.RefresPressureProtectionModifiers((partEntity, pressureProtection));
                if (TryComp<TemperatureProtectionComponent>(partEntity, out var temperatureProtection))
                    _temperatureSystem.RefresTempratureProtectionModifiers((partEntity, temperatureProtection));
                if (TryComp<ModsuitClothingComponent>(partEntity, out var modComp))
                    modComp.ValuesChanged = false;
            }
        }
    }
    private void OnPressureProtectionEvent(Entity<PressureProtectionComponent> ent, ref RefreshPressureProtectiondModifiersEvent args)
    {
        if (!TryComp<ModsuitClothingComponent>(ent, out var comp))
            return;
        args.ModifyProtection(
            (comp.ValuesChanged ? -1 : 1) * comp.LowPressureModifier,
            comp.ValuesChanged ? 1f / comp.LowPressureMultiplier : comp.LowPressureMultiplier,
            (comp.ValuesChanged ? -1 : 1) * comp.HighPressureModifier,
            comp.ValuesChanged ? 1f / comp.HighPressureMultiplier : comp.HighPressureMultiplier
        );
    }
    private void OnTempratureProtectionEvent(Entity<TemperatureProtectionComponent> ent, ref RefreshTempratureProtectiondModifiersEvent args)
    {
        if (!TryComp<ModsuitClothingComponent>(ent, out var comp))
            return;
        args.ModifyProtection(
            (comp.ValuesChanged ? 1 : -1) * comp.CoefficientModifier
        );
    }
}
