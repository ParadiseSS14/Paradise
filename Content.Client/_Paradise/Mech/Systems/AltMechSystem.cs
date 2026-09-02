using Content.Client._Paradise.Mech.Ui;
using Content.Client.UserInterface.Systems.DamageOverlays.Overlays;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Paradise.Mech;
using Content.Shared.Paradise.Mech.Components;
using Content.Shared.Paradise.Mech.Parts.Components;
using Content.Shared.Paradise.Mech.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._Paradise.Mech;

public sealed partial class AltMechSystem : SharedAltMechSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private IOverlayManager _overlay = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private DamageOverlay _damageOverlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AltMechComponent, AppearanceChangeEvent>(OnAppearanceChanged);

        SubscribeLocalEvent<AltMechComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MechPartComponent, DamageChangedEvent>(OnPartDamageChanged);

        SubscribeLocalEvent<AltMechComponent, OnMechExitEvent>(OnPilotEjected);

        _damageOverlay = new DamageOverlay();
        SubscribeLocalEvent<AltMechComponent, LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<AltMechComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<AltMechPilotComponent, MobThresholdChecked>(OnThresholdCheck);

    }

    private void OnAppearanceChanged(Entity<AltMechComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_sprite.LayerExists((ent.Owner, args.Sprite), MechVisualLayers.Base))
            return;

        var drawDepth = DrawDepth.Mobs;

        _sprite.LayerSetRsiState((ent.Owner, args.Sprite), MechVisualLayers.Base, ent.Comp.BaseState);
        _sprite.SetDrawDepth((ent.Owner, args.Sprite), (int)drawDepth);
    }

    protected override void OnStartup(Entity<AltMechComponent> ent, ref ComponentStartup args)
    {
        base.OnStartup(ent, ref args);

        if (!TryComp<SpriteComponent>(ent.Owner, out var spriteComp) || !TryComp(ent, out AppearanceComponent? appearance))
            return;

        _sprite.LayerSetColor((ent, spriteComp), ent.Comp.AttachedColoredSpriteLayer, ent.Comp.ColoredSpriteColor);

        foreach (var partContainer in ent.Comp.ContainerDict)
        {
            if (partContainer.Value.ContainedEntity is not { Valid: true } partEntityValid || !TryComp<MechPartComponent>(partEntityValid, out var partComp))
            {
                if (_sprite.LayerMapTryGet((ent.Owner, spriteComp), PartsVisuals[partContainer.Key], out var layerOfMissingPart, true))
                    _sprite.LayerSetVisible((ent.Owner, spriteComp), layerOfMissingPart, false);

                continue;
            }

            ProcessPartVisuals(ent, (partEntityValid, partComp), true, partContainer.Key);
        }
    }

    protected override void OnEntityInserted(Entity<AltMechComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        base.OnEntityInserted(ent, ref args);

        string containerID = args.Container.ID;

        if (!containerID.StartsWith(PartContainerPrefix))
            return;

        if (TryComp<MechPartComponent>(args.Entity, out var partComp))
            ProcessPartVisuals(ent, (args.Entity, partComp), true, partComp.Slot);

        if (!TryComp<UserInterfaceComponent>(ent, out var uiComp))
            return;

        if (uiComp.ClientOpenInterfaces.ContainsKey(MechUiKey.Key) && uiComp.ClientOpenInterfaces[MechUiKey.Key] is AltMechBoundUserInterface)
        {
            var bui = (AltMechBoundUserInterface)uiComp.ClientOpenInterfaces[MechUiKey.Key];

            if (bui == null)
                return;

            bui.UpdateUI();
        }
    }

    protected override void OnEntityRemoved(Entity<AltMechComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        base.OnEntityRemoved(ent, ref args);

        string containerID = args.Container.ID;

        if (!containerID.StartsWith(PartContainerPrefix))
            return;

        if (TryComp<MechPartComponent>(args.Entity, out var partComp))
            ProcessPartVisuals(ent, (args.Entity, partComp), false, partComp.Slot);

        if (!TryComp<UserInterfaceComponent>(ent, out var uiComp))
            return;

        if (uiComp.ClientOpenInterfaces.ContainsKey(MechUiKey.Key) && uiComp.ClientOpenInterfaces[MechUiKey.Key] is AltMechBoundUserInterface)
        {
            var bui = (AltMechBoundUserInterface)uiComp.ClientOpenInterfaces[MechUiKey.Key];

            if (bui == null)
                return;

            bui.UpdateUI();
        }

    }

    private void OnDamageChanged(Entity<AltMechComponent> ent, ref DamageChangedEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(ent, out var uiComp))
            return;

        if (uiComp.ClientOpenInterfaces.ContainsKey(MechUiKey.Key) && uiComp.ClientOpenInterfaces[MechUiKey.Key] is AltMechBoundUserInterface)
        {
            var bui = (AltMechBoundUserInterface)uiComp.ClientOpenInterfaces[MechUiKey.Key];

            if (bui == null)
                return;

            bui.UpdateUI();
        }

    }

    private void OnPartDamageChanged(Entity<MechPartComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.PartOwner == null)
            return;

        var mech = (EntityUid)ent.Comp.PartOwner;

        if (mech != _playerManager.LocalEntity)
            return;

        if (!TryComp<UserInterfaceComponent>(mech, out var uiComp))
            return;

        if (uiComp.ClientOpenInterfaces.ContainsKey(MechUiKey.Key) && uiComp.ClientOpenInterfaces[MechUiKey.Key] is AltMechBoundUserInterface)
        {
            var bui = (AltMechBoundUserInterface)uiComp.ClientOpenInterfaces[MechUiKey.Key];

            if (bui == null)
                return;

            bui.UpdateUI();
        }

    }

    private void ProcessPartVisuals(Entity<AltMechComponent> mech, Entity<MechPartComponent> part, bool attached, PartSlot slot)
    {
        if (!TryComp<SpriteComponent>(mech, out var spriteComp) || spriteComp == null)
            return;

        SpriteSpecifier? spriteToAdd = part.Comp.AttachedSprite;

        SpriteSpecifier? coloredSpriteToAdd = part.Comp.AttachedColoredSprite;

        if (slot == PartSlot.Head)
        {
            _sprite.LayerSetVisible((mech, spriteComp), mech.Comp.AttachedHeadSpriteLayer, attached);
            _sprite.LayerSetVisible((mech, spriteComp), mech.Comp.AttachedHeadColoredSpriteLayer, attached);
            _sprite.LayerSetVisible((mech, spriteComp), mech.Comp.CameraVisLayer, attached);

            _sprite.LayerSetColor((mech, spriteComp), mech.Comp.AttachedHeadColoredSpriteLayer, part.Comp.ColoredSpriteColor);

            if (TryComp<MechOpticsComponent>(part, out var opticsComp))
                _sprite.LayerSetColor((mech, spriteComp), mech.Comp.CameraVisLayer, opticsComp.CameraLayerColor);

            return;
        }

        if (_sprite.LayerMapTryGet((mech, spriteComp), PartsVisuals[part.Comp.Slot], out var layer, true))
        {
            _sprite.LayerSetVisible((mech, spriteComp), layer, attached);
            if (attached)
            {
                if (spriteToAdd != null)
                    _sprite.LayerSetSprite((mech, spriteComp), layer, spriteToAdd);
            }
        }

        if (coloredSpriteToAdd != null && _sprite.LayerMapTryGet((mech, spriteComp), PartsVisuals[part.Comp.Slot] + 1, out var layerColored, true))
        {
            _sprite.LayerSetVisible((mech, spriteComp), layerColored, attached);
            if (attached)
            {
                if (coloredSpriteToAdd != null)
                    _sprite.LayerSetSprite((mech, spriteComp), layerColored, coloredSpriteToAdd);

                _sprite.LayerSetColor((mech, spriteComp), layerColored, part.Comp.ColoredSpriteColor);
            }
        }
    }

    protected override void OnMechInteractedWith(Entity<AltMechComponent> ent, ref AfterInteractUsingEvent args)
    {
        base.OnMechInteractedWith(ent, ref args);

        if (TryComp<SpriteComponent>(ent.Owner, out var spriteComp))
            _sprite.LayerSetColor((ent, spriteComp), ent.Comp.AttachedColoredSpriteLayer, ent.Comp.ColoredSpriteColor);
    }

    private void OnPilotEjected(Entity<AltMechComponent> ent, ref OnMechExitEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(ent, out var uiComp))
            return;

        if (uiComp.ClientOpenInterfaces.ContainsKey(MechUiKey.Key) && uiComp.ClientOpenInterfaces[MechUiKey.Key] is AltMechBoundUserInterface)
        {
            var bui = (AltMechBoundUserInterface)uiComp.ClientOpenInterfaces[MechUiKey.Key];

            if (bui == null)
                return;

            bui.Close();
        }
    }
}
