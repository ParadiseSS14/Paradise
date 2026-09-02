using Content.Shared._Paradise.ArmorBlock;
using Content.Shared.Access.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Flash;
using Content.Shared.Flash.Components;
using Content.Shared.Gravity;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Paradise.Mech.Components;
using Content.Shared.Paradise.Mech.Equipment.Components;
using Content.Shared.Paradise.Mech.Parts.Components;
using Content.Shared.Popups;
using Content.Shared.Radio.Components;
using Content.Shared.Random.Helpers;
using Content.Shared.SprayPainter.Components;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Storage.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared.Paradise.Mech.Systems;

public abstract partial class SharedAltMechSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private BlindableSystem _blindable = default!;
    [Dependency] private MetaDataSystem _meta = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    public EntProtoId PilotEjectAction = "ActionMechEject";
    public EntProtoId MechUIOpenAction = "ActionMechOpenUI";
    public EntProtoId CombatModeToggleAction = "ActionCombatModeToggle";
    public EntProtoId MechRelayAction = "ActionMechRelay";

    public static readonly LocId MechArmTooHeavy = "mech-arm-too-heavy";

    public static readonly string PartContainerPrefix = "mech_part";

    public readonly Dictionary<PartSlot, MechPartVisualLayers> PartsVisuals = new Dictionary<PartSlot, MechPartVisualLayers>()
    {
        [PartSlot.Head] = MechPartVisualLayers.Head,
        [PartSlot.RightArm] = MechPartVisualLayers.RightArm,
        [PartSlot.LeftArm] = MechPartVisualLayers.LeftArm,
        [PartSlot.Chassis] = MechPartVisualLayers.Chassis,
        [PartSlot.Power] = MechPartVisualLayers.Power
    };

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<AltMechComponent, MechEjectPilotEvent>(OnEjectPilotEvent);
        SubscribeLocalEvent<AltMechComponent, MechRelayActionEvent>(OnMechRelayEvent);

        SubscribeLocalEvent<AltMechComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AltMechComponent, EntityStorageIntoContainerAttemptEvent>(OnEntityStorageDump);
        SubscribeLocalEvent<AltMechComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
        SubscribeLocalEvent<AltMechComponent, DragDropTargetEvent>(OnDragDrop);
        SubscribeLocalEvent<AltMechComponent, CanDropTargetEvent>(OnCanDragDrop);

        SubscribeLocalEvent<AltMechComponent, EntInsertedIntoContainerMessage>(OnEntityInserted);
        SubscribeLocalEvent<AltMechComponent, EntRemovedFromContainerMessage>(OnEntityRemoved);

        SubscribeLocalEvent<AltMechComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeedMod);

        SubscribeLocalEvent<AltMechComponent, MechPilotRelayedEvent<FlashAttemptEvent>>(OnPilotFlashed);
        SubscribeLocalEvent<AltMechComponent, FlashAttemptEvent>(OnMechFlashed);
        SubscribeLocalEvent<AltMechComponent, GetEyeProtectionEvent>(OnMechGetEyeProtection);
        SubscribeLocalEvent<AltMechComponent, AfterInteractUsingEvent>(OnMechInteractedWith);

        SubscribeLocalEvent<AltMechComponent, IsWeightlessEvent>(OnWeightlessCheck);

        SubscribeLocalEvent<AltMechPilotComponent, StatusEffectAppliedToEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<AltMechPilotComponent, StatusEffectRemovedFromEvent>(OnStatusEffectRemoved);

        SubscribeLocalEvent<AltMechComponent, ProjectileBlockAttemptEvent>(OnProjectileHit);
        SubscribeLocalEvent<AltMechComponent, HitscanBlockAttemptEvent>(OnHitscan);
        SubscribeLocalEvent<AltMechComponent, MeleeHitBlockAttemptEvent>(OnMeleeHit);
        SubscribeLocalEvent<AltMechComponent, ThrowableProjectileBlockAttemptEvent>(OnThrownProjectileHit);

        InitializeRelay();
    }

    private void OnEjectPilotEvent(Entity<AltMechComponent> ent, ref MechEjectPilotEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var ev = new OnMechExitEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    protected virtual void OnEntityInserted(Entity<AltMechComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        string containerID = args.Container.ID;

        if (TryComp<MechPartComponent>(args.Entity, out var partComp) && containerID.StartsWith(PartContainerPrefix))
        {
            var ev = new MechPartInsertedEvent(ent.Owner);
            RaiseLocalEvent(args.Entity, ref ev);

            AddMass(ent, partComp.OwnMass);

            var massEv = new MassChangedEvent();
            RaiseLocalEvent(ent, ref massEv);

            Dirty(ent);
            return;
        }

        if (TryComp<AltMechEquipmentComponent>(args.Entity, out var moduleComp) && containerID.StartsWith(ent.Comp.EquipmentContainerId))
        {
            var ev = new MechEquipmentInsertedEvent(ent.Owner);
            RaiseLocalEvent(args.Entity, ref ev);

            AddMass(ent, moduleComp.OwnMass);

            var massEv = new MassChangedEvent();
            RaiseLocalEvent(ent, ref massEv);
        }

        Dirty(ent);
    }

    protected virtual void OnEntityRemoved(Entity<AltMechComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        string containerID = args.Container.ID;

        if (TryComp<MechPartComponent>(args.Entity, out var partComp) && containerID.StartsWith(PartContainerPrefix))
        {
            var ev = new MechPartRemovedEvent(ent.Owner);
            RaiseLocalEvent(args.Entity, ref ev);

            RemoveMass(ent, partComp.OwnMass);

            var massEv = new MassChangedEvent();
            RaiseLocalEvent(ent, ref massEv);

            Dirty(ent);
            return;
        }

        if (TryComp<AltMechEquipmentComponent>(args.Entity, out var moduleComp) && containerID.StartsWith(ent.Comp.EquipmentContainerId))
        {
            var ev = new MechEquipmentRemovedEvent(ent.Owner);
            RaiseLocalEvent(args.Entity, ref ev);

            RemoveMass(ent, moduleComp.OwnMass);

            var massEv = new MassChangedEvent();
            RaiseLocalEvent(ent, ref massEv);
        }

        Dirty(ent);
    }

    private void OnRefreshMoveSpeedMod(Entity<AltMechComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<MovementSpeedModifierComponent>(ent.Owner, out var movementComp))
            return;

        FixedPoint2 maxMass = 1;

        if (!ent.Comp.ContainerDict.TryGetValue(PartSlot.Chassis, out var chassisContainer))
            return;

        if (TryComp<MechChassisComponent>(chassisContainer.ContainedEntity, out var chassisComp))
            maxMass = chassisComp.MaximalMass;

        var massDiff = ent.Comp.OverallMass - maxMass;

        if (massDiff < 0)
            massDiff = 0;

        FixedPoint2 massRel = 1 - massDiff / maxMass;

        ent.Comp.MovementSpeedModifier = Math.Clamp(massRel.Float(), 0, 1);

        args.ModifySpeed(ent.Comp.MovementSpeedModifier, ent.Comp.MovementSpeedModifier);
    }

    private void OnMechRelayEvent(Entity<AltMechComponent> ent, ref MechRelayActionEvent args)
    {
        if (ent.Comp.PilotSlot.ContainedEntity is not { Valid: true } pilot)
            return;

        if (_net.IsServer)
        {
            var request = new RequestPerformActionEvent(GetNetEntity(args.ActionToPerform));

            _actions.TryPerformAction(request, pilot);
        }

        if (TryComp<ActionComponent>(args.Action, out var actionComp) &&
            TryComp<ActionComponent>(args.ActionToPerform, out var addedActionComp) &&
            addedActionComp.Cooldown != null)
        {
            _actions.SetCooldown((args.Action, actionComp), addedActionComp.Cooldown.Value.End - addedActionComp.Cooldown.Value.Start);
            Dirty(args.Action, actionComp);
        }
    }

    private void OnPilotFlashed(Entity<AltMechComponent> ent, ref MechPilotRelayedEvent<FlashAttemptEvent> args)
    {
        if (TryComp<FlashImmunityComponent>(ent.Owner, out var _))
        {
            args.Args.Cancelled = true;
            return;
        }
        RelayRefToParts(ent, ref args);
        RelayRefToEquipment(ent, ref args);
    }

    private void OnMechFlashed(Entity<AltMechComponent> ent, ref FlashAttemptEvent args)
    {
        if (TryComp<FlashImmunityComponent>(ent.Owner, out var _))
        {
            args.Cancelled = true;
            return;
        }
        RelayRefToParts(ent, ref args);
        RelayRefToEquipment(ent, ref args);
    }

    private void OnMechGetEyeProtection(Entity<AltMechComponent> ent, ref GetEyeProtectionEvent args)
    {
        if (ent.Comp.ContainerDict[PartSlot.Head].ContainedEntity == null)
            return;

        if (TryComp<EyeProtectionComponent>(ent.Comp.ContainerDict[PartSlot.Head].ContainedEntity, out var immunityComp))
            args.Protection += immunityComp.ProtectionTime;
    }

    protected virtual void OnStartup(Entity<AltMechComponent> ent, ref ComponentStartup args)
    {
        foreach (PartSlot part in Enum.GetValues(typeof(PartSlot)))
        {
            if (part == PartSlot.Core)
                continue;

            ent.Comp.ContainerDict[part] = _container.EnsureContainer<ContainerSlot>(ent.Owner, PartContainerPrefix + "_" + part);
        }

        ent.Comp.PilotSlot = _container.EnsureContainer<ContainerSlot>(ent.Owner, ent.Comp.PilotSlotId);

        ent.Comp.TankSlot = _container.EnsureContainer<ContainerSlot>(ent.Owner, ent.Comp.TankSlotId);

        ent.Comp.EquipmentContainer = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.EquipmentContainerId);

        ent.Comp.OverallMass += ent.Comp.OwnMass;

        ent.Comp.Integrity = ent.Comp.MaxIntegrity;

        if (TryComp<MovementSpeedModifierComponent>(ent.Owner, out var movementComp))
            _movementSpeedModifier.RefreshMovementModifiers(ent.Owner);

        if (ent.Comp.ContainerDict[PartSlot.Head].ContainedEntity == null && !ent.Comp.Transparent)
        {
            TryComp<BlindableComponent>(ent.Owner, out var blindableComp);
            _blindable.AdjustEyeDamage((ent.Owner, blindableComp), 9); //Mech cannot see anything if it has no eyes
        }

        _actions.AddAction(ent.Owner, ref ent.Comp.MechUiActionEntity, ent.Comp.MechUiAction, ent.Owner);
        _actions.AddAction(ent.Owner, ref ent.Comp.MechEjectActionEntity, ent.Comp.MechEjectAction, ent.Owner);
    }

    public virtual void OnStartupServer(Entity<AltMechComponent> ent)
    {

    }

    private void OnEntityStorageDump(Entity<AltMechComponent> entity, ref EntityStorageIntoContainerAttemptEvent args)
    {
        // There's no reason we should dump into /any/ of the mech's containers.
        args.Cancelled = true;
    }

    private void OnGetAdditionalAccess(Entity<AltMechComponent> ent, ref GetAdditionalAccessEvent args)
    {
        var pilot = ent.Comp.PilotSlot.ContainedEntity;
        if (pilot == null)
            return;

        args.Entities.Add(pilot.Value);
    }

    protected virtual void OnMechInteractedWith(Entity<AltMechComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!TryComp<SprayPainterComponent>(args.Used, out var painterComp) || painterComp.SelectedDecalColor == null)
            return;

        if (painterComp.SelectedDecalColor != null)
        {
            ent.Comp.ColoredSpriteColor = (Color)painterComp.SelectedDecalColor;
            return;
        }

        if (painterComp.ColorPalette.ContainsKey(painterComp.PickedColor))
            ent.Comp.ColoredSpriteColor = painterComp.ColorPalette[painterComp.PickedColor];
    }

    private void OnProjectileHit(Entity<AltMechComponent> ent, ref ProjectileBlockAttemptEvent args)
    {
        if (args.Damage != null)
            args.Cancelled = AttackHandle(ent, args.Damage);
    }

    private void OnMeleeHit(Entity<AltMechComponent> ent, ref MeleeHitBlockAttemptEvent args)
    {
        if (MeleeAttackHandle(ent, out var part) && part is { Valid: true } partValidated)
        {
            args.Blocker = partValidated;
            args.Cancelled = true;
        }
    }

    private void OnHitscan(Entity<AltMechComponent> ent, ref HitscanBlockAttemptEvent args)
    {
        if (args.Damage != null)
            args.Cancelled = AttackHandle(ent, args.Damage);
    }

    private void OnThrownProjectileHit(Entity<AltMechComponent> ent, ref ThrowableProjectileBlockAttemptEvent args)
    {
        if (args.Damage != null)
            args.Cancelled = AttackHandle(ent, args.Damage);
    }

    private bool AttackHandle(Entity<AltMechComponent> ent, DamageSpecifier damage)
    {
        if (!TryGetNetEntity(ent.Owner, out var netMech))
            return false;

        foreach (var part in ent.Comp.ContainerDict)
        {
            if (part.Key == PartSlot.Power || part.Value == null || part.Value.ContainedEntity is not { Valid: true } partValid)
                continue;

            if (!TryGetNetEntity(partValid, out var netItem))
                continue;

            //if (SharedRandomExtensions.PredictedProb(_timing, 0.16f, (NetEntity)NetMech, (NetEntity)NetItem))//this chance is hardcoded because using mech parts as shields is not planned, it's just a patch to make it work untill part damage UI is made

            if (SharedRandomExtensions.PredictedProb(_timing, 0.16f, (NetEntity)netItem))
            {
                _damageable.TryChangeDamage(partValid, damage);
                return true;
            }
        }

        return false;
    }

    private bool MeleeAttackHandle(Entity<AltMechComponent> ent, out EntityUid? targetedPart)
    {
        if (!TryGetNetEntity(ent.Owner, out var netMech))
        {
            targetedPart = null;
            return false;
        }

        foreach (var part in ent.Comp.ContainerDict)
        {
            if (part.Key == PartSlot.Power || part.Value == null || part.Value.ContainedEntity is not { Valid: true } partValid)
                continue;

            if (!TryGetNetEntity(partValid, out var netItem))
                continue;

            //if (SharedRandomExtensions.PredictedProb(_timing, 0.16f, (NetEntity)NetMech, (NetEntity)NetItem))//this chance is hardcoded because using mech parts as shields is not planned, it's just a patch to make it work untill part damage UI is made

            if (SharedRandomExtensions.PredictedProb(_timing, 0.16f, (NetEntity)netItem))
            {
                targetedPart = partValid;
                return true;
            }
        }

        targetedPart = null;
        return false;
    }


    private void SetupUser(Entity<AltMechComponent> mech, EntityUid pilot)
    {
        var pilotComp = EnsureComp<AltMechPilotComponent>(pilot);

        pilotComp.Mech = mech;

        if (TryComp<BlindableComponent>(pilot, out var blindableCompPilot))
        {
            pilotComp.PilotEyeDamage = blindableCompPilot.EyeDamage;
            _blindable.AdjustEyeDamage(pilot, 9 - blindableCompPilot.EyeDamage);
        }

        if (_net.IsClient)
            return;

        var ev = new DropHandItemsEvent();
        RaiseLocalEvent(pilot, ref ev);

        RadioVoiceSetup(mech, pilot);

        if (!TryComp<ActionsComponent>(mech.Owner, out var mechActions))
            return;

        foreach (var action in mechActions.Actions.ToArray())
        {
            var actionMeta = MetaData(action);
            if (actionMeta.EntityPrototype != null &&
                actionMeta.EntityPrototype.ID == MechRelayAction)
                _actions.RemoveAction(action);
        }

        EffectsSetup(mech.Owner, pilot);

        if (!TryComp<ActionsComponent>(pilot, out var pilotActions))
            return;

        foreach (var action in pilotActions.Actions.ToArray())
        {
            var actionMeta = MetaData(action);

            if (!TryComp<ActionComponent>(action, out var actionComp) || actionMeta.EntityPrototype != null && actionMeta.EntityPrototype.ID == CombatModeToggleAction)
                continue;

            var container = actionComp.Container != null ? actionComp.Container : mech.Owner;

            if (container is not { Valid: true } containerValidated)
                continue;

            var addedAction = _actions.AddAction(mech.Owner, MechRelayAction, containerValidated);

            if (addedAction is not { Valid: true } addedActionValidated || !TryComp<ActionComponent>(addedActionValidated, out var addedActionComp))
                continue;

            _actions.SetEntityIcon((addedActionValidated, addedActionComp), actionComp.EntIcon);

            if (actionComp.Cooldown != null)
                _actions.SetCooldown((addedActionValidated, addedActionComp), actionComp.Cooldown.Value.End - actionComp.Cooldown.Value.Start);

            _actions.SetStyle((addedActionValidated, addedActionComp), actionComp.ItemIconStyle);

            _meta.SetEntityName(addedActionValidated, actionMeta.EntityName);
            _meta.SetEntityDescription(addedActionValidated, actionMeta.EntityDescription);

            var eventToSet = new MechRelayActionEvent();

            eventToSet.ActionToPerform = action;
            eventToSet.ActionUser = pilot;

            eventToSet.Action = (addedActionValidated, addedActionComp);

            _actions.SetEvent(addedActionValidated, eventToSet);
        }

        _actions.AddAction(pilot, ref pilotComp.PilotUiActionEntity, pilotComp.PilotUiAction, mech);
        _actions.AddAction(pilot, ref pilotComp.PilotEjectActionEntity, pilotComp.PilotEjectAction, mech);
    }

    public void EffectsSetup(EntityUid mech, EntityUid pilot)
    {
        if (!TryComp<StatusEffectContainerComponent>(pilot, out var pilotEffects))
            return;

        if (pilotEffects.ActiveStatusEffects is not { } containerPilot)
            return;

        foreach (var effect in containerPilot.ContainedEntities)
        {
            if (!TryComp<StatusEffectComponent>(effect, out var effectComp))
                continue;

            var effectMeta = MetaData(effect);

            if (effectMeta.EntityPrototype == null)
                continue;

            _statusEffects.TrySetStatusEffectDuration(mech, effectMeta.EntityPrototype, effectComp.Duration);
        }
    }

    public void RadioVoiceSetup(EntityUid mech, EntityUid pilot)
    {
        if (TryComp<ActiveRadioComponent>(mech, out var mechRadio))
        {
            if (TryComp<InventoryComponent>(pilot, out var pilotInventory) && _inventory.TryGetSlotContainer(pilot, "ears", out var slot, out var def))
            {
                if (!TryComp<ActiveRadioComponent>(slot.ContainedEntity, out var radioComp))
                    return;
                mechRadio.Channels = new (radioComp.Channels);
            }
            if (TryComp<ActiveRadioComponent>(pilot, out var embeddedRadio))//in case the pilot is a radio himself
            {
                foreach (var channel in embeddedRadio.Channels)
                    mechRadio.Channels.Add(channel);
            }
        }
    }

    public virtual void BreakMech(Entity<AltMechComponent> ent)
    {
        TryEject(ent);
        var equipment = new List<EntityUid>(ent.Comp.EquipmentContainer.ContainedEntities);

        ent.Comp.Broken = true;
    }

    public void InsertEquipment(EntityUid uid, EntityUid toInsert, AltMechComponent? component = null,
        AltMechEquipmentComponent? equipmentComponent = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!Resolve(toInsert, ref equipmentComponent))
            return;

        if (component.MaxEquipmentAmount < component.CurrentEquipmentAmount + equipmentComponent.EqipmentSize)
            return;

        if (_whitelistSystem.IsWhitelistFail(component.EquipmentWhitelist, toInsert))
            return;

        component.CurrentEquipmentAmount += equipmentComponent.EqipmentSize;

        equipmentComponent.EquipmentOwner = uid;

        Dirty(uid, component);
        Dirty(toInsert, equipmentComponent);

        _container.Insert(toInsert, component.EquipmentContainer);

        var ev = new MechEquipmentInsertedEvent(uid);
        RaiseLocalEvent(toInsert, ref ev);
    }

    public void InsertPart(EntityUid uid, EntityUid toInsert)
    {
        if (!TryComp<AltMechComponent>(uid, out var component))
            return;

        if (!component.MaintenanceMode)
            return;

        if (!TryComp<MechPartComponent>(toInsert, out var partComponent))
            return;

        if (!component.ContainerDict.ContainsKey(partComponent.Slot) || component.ContainerDict[partComponent.Slot].ContainedEntity != null)
            return;

        if (TryComp<MechArmComponent>(toInsert, out var armComp) &&
            partComponent.OwnMass > component.MaximalArmMass)
        {
            _popup.PopupEntity(Loc.GetString(MechArmTooHeavy), uid);
            return;
        }

        partComponent.PartOwner = uid;
        _container.Insert(toInsert, component.ContainerDict[partComponent.Slot]);

        Dirty(uid, component);
        Dirty(toInsert, partComponent);

        Dirty<AltMechComponent>((uid, component));
    }

    public void AddMass(Entity<AltMechComponent> ent, FixedPoint2 value)
    {
        ent.Comp.OverallMass += value;
    }

    public void RemoveMass(Entity<AltMechComponent> ent, FixedPoint2 value)
    {
        ent.Comp.OverallMass -= value;
    }

    public void RemoveEquipment(EntityUid uid, EntityUid toRemove)
    {
        if (!TryComp<AltMechComponent>(uid, out var mechComp))
            return;

        if (!TryComp<AltMechEquipmentComponent>(toRemove, out var equipmentComponent))
            return;

        if (equipmentComponent.EquipmentOwner != uid)
            return;

        if (equipmentComponent != null)
        {
            mechComp.CurrentEquipmentAmount -= equipmentComponent.EqipmentSize;
            equipmentComponent.EquipmentOwner = null;
            Dirty(uid, mechComp);
            Dirty(toRemove, equipmentComponent);
        }

        _container.Remove(toRemove, mechComp.EquipmentContainer);

        var ev = new MechEquipmentRemovedEvent(uid);
        RaiseLocalEvent(toRemove, ref ev);
    }

    public void RemovePart(EntityUid uid, EntityUid toRemove)
    {
        if (!TryComp<AltMechComponent>(uid, out var component))
            return;

        if (!TryComp<MechPartComponent>(toRemove, out var partComponent))
            return;

        if (partComponent == null)
            return;

        if (!component.ContainerDict.ContainsKey(partComponent.Slot) || component.ContainerDict[partComponent.Slot].ContainedEntity == null)
            return;

        PartSlot slot;

        if (partComponent == null)
            return;

        slot = partComponent.Slot;
        partComponent.PartOwner = null;

        _container.Remove(toRemove, component.ContainerDict[partComponent.Slot]);

        Dirty(toRemove, partComponent);

        Dirty(uid, component);

        //if (TryGetNetEntity(uid, out var netMech) && TryGetNetEntity(toRemove, out var netPart))
        //{
        //    RaiseNetworkEvent(new MechPartStatusChanged((NetEntity)netMech, (NetEntity)netPart, false, slot));
        //    Dirty<AltMechComponent>((uid, component));
        //}

        Dirty<AltMechComponent>((uid, component));
    }

    /// <summary>
    /// Attempts to change the amount of energy in the mech.
    /// </summary>
    /// <param name="uid">The mech itself</param>
    /// <param name="delta">The change in energy</param>
    /// <param name="component"></param>
    /// <returns>If the energy was successfully changed.</returns>
    public virtual bool TryChangeEnergy(Entity<AltMechComponent> ent, FixedPoint2 delta)
    {
        if (!HasComp<AltMechComponent>(ent))
            return false;

        if (ent.Comp.Energy + delta < 0)
            return false;

        ent.Comp.Energy = FixedPoint2.Clamp(ent.Comp.Energy + delta, 0, ent.Comp.MaxEnergy);
        Dirty(ent);
        return true;
    }

    /// <summary>
    /// Sets the integrity of the mech.
    /// </summary>
    /// <param name="uid">The mech itself</param>
    /// <param name="value">The value the integrity will be set at</param>
    /// <param name="component"></param>
    public virtual void SetIntegrity(Entity<AltMechComponent> ent, FixedPoint2 value)
    {
        ent.Comp.Integrity = FixedPoint2.Clamp(value, 0, ent.Comp.MaxIntegrity);

        if (ent.Comp.Integrity >= 0 &&
            ent.Comp.Broken)
            ent.Comp.Broken = false;

        Dirty(ent);
    }

    /// <summary>
    /// Checks if the pilot is present
    /// </summary>
    /// <param name="component"></param>
    /// <param name="uid"></param>
    /// <returns>Whether or not the pilot is present</returns>
    public bool IsEmpty(AltMechComponent component)
    {
        return component.PilotSlot.ContainedEntity == null;
    }

    /// <summary>
    /// Checks if an entity can be inserted into the mech.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="toInsert"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public bool CanInsert(Entity<AltMechComponent> ent, EntityUid toInsert)
    {
        if (!HasComp<AltMechComponent>(ent))
            return false;

        return IsEmpty(ent.Comp) && !ent.Comp.Bolted;
    }

    /// <summary>
    /// Attempts to insert a pilot into the mech.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="toInsert"></param>
    /// <param name="component"></param>
    /// <returns>Whether or not the entity was inserted</returns>
    public bool TryInsert(Entity<AltMechComponent> ent, EntityUid? toInsert)
    {
        if (toInsert is not { Valid: true } toInsertValid || ent.Comp.PilotSlot.ContainedEntity == toInsertValid)
            return false;

        if (TryComp<InventoryComponent>(toInsertValid, out var inventoryComp))
        {
            foreach (var slot in ent.Comp.SlotsToDrop)
            {
                _inventory.TryUnequip(toInsertValid, slot);
            }
        }

        if (!CanInsert(ent, toInsertValid))
            return false;

        SetupUser(ent, toInsertValid);
        _container.Insert(toInsertValid, ent.Comp.PilotSlot);

        var ev = new OnMechEntryEvent();
        RaiseLocalEvent(ent, ref ev);

        if (TryComp<ArmorBlockComponent>(ent, out var blockComp))
            blockComp.User = toInsertValid;
        return true;
    }

    /// <summary>
    /// Attempts to eject the current pilot from the mech
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <returns>Whether or not the pilot was ejected.</returns>
    public virtual bool TryEject(Entity<AltMechComponent> ent)
    {
        if (ent.Comp.PilotSlot.ContainedEntity == null || (ent.Comp.Bolted && !ent.Comp.BoltsSawed))
            return false;

        var pilot = ent.Comp.PilotSlot.ContainedEntity.Value;

        if (!TryComp<AltMechPilotComponent>(pilot, out var pilotComp))
            return false;

        if (TryComp<ActiveRadioComponent>(ent.Owner, out var mechRadio))
        {
            mechRadio.Channels.Clear();
        }

        if (pilotComp.PilotUiActionEntity is { Valid: true } pilotUiActionValid)
            _actions.RemoveProvidedAction(pilot, ent.Owner, pilotUiActionValid);

        if (pilotComp.PilotEjectActionEntity is { Valid: true } pilotEjectActionValid)
            _actions.RemoveProvidedAction(pilot, ent.Owner, pilotEjectActionValid);

        _container.RemoveEntity(ent.Owner, pilot);

        if (TryComp<BlindableComponent>(pilot, out var blindableCompPilot))
            _blindable.AdjustEyeDamage(pilot, pilotComp.PilotEyeDamage - blindableCompPilot.EyeDamage);

        if (!RemComp<AltMechPilotComponent>(pilot))
            return false;

        if (TryComp<ArmorBlockComponent>(ent.Owner, out var blockComp))
            blockComp.User = null;

        return true;
    }

    private void OnDragDrop(Entity<AltMechComponent> ent, ref DragDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Dragged, ent.Comp.EntryDelay, new MechEntryEvent(), ent.Owner, target: ent.Owner)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    public void OnWeightlessCheck(Entity<AltMechComponent> ent, ref IsWeightlessEvent args)
    {
        RelayRefToParts(ent, ref args);
        RelayRefToEquipment(ent, ref args);
    }

    public void OnStatusEffectApplied(Entity<AltMechPilotComponent> ent, ref StatusEffectAppliedToEvent args)
    {
        if (!TryComp<StatusEffectComponent>(args.Effect, out var effectComp))
            return;

        var effectMeta = MetaData(args.Effect);

        if (effectMeta.EntityPrototype == null)
            return;

        _statusEffects.TrySetStatusEffectDuration(ent.Comp.Mech, effectMeta.EntityPrototype, effectComp.Duration);
    }

    public void OnStatusEffectRemoved(Entity<AltMechPilotComponent> ent, ref StatusEffectRemovedFromEvent args)
    {
    }

    private void OnCanDragDrop(Entity<AltMechComponent> ent, ref CanDropTargetEvent args)
    {
        args.Handled = true;

        args.CanDrop = CanInsert(ent, args.Dragged);
    }

}
