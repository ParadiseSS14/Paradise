using Content.Shared._Paradise.ItemExtension;
using Content.Shared._Paradise.PhysicalParameters;
using Content.Shared.Clothing;
using Content.Shared.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Item;

/// <summary>
/// This handles <see cref="HeldSpeedModifierComponent"/>
/// </summary>
public sealed partial class HeldSpeedModifierSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private PhysicalParametersSystem _parameters = default!;// PARADISE EDIT - Physical parameters

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<HeldSpeedModifierComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<HeldSpeedModifierComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<HeldSpeedModifierComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
        SubscribeLocalEvent<HeldSpeedModifierComponent, HeldRelayedEvent<RefreshWeightlessModifiersEvent>>(OnRefreshWeightlessModifiers);
    }

    private void OnGotEquippedHand(Entity<HeldSpeedModifierComponent> ent, ref GotEquippedHandEvent args)
    {
        _movementSpeedModifier.RefreshMovementModifiers(args.User);
    }

    private void OnGotUnequippedHand(Entity<HeldSpeedModifierComponent> ent, ref GotUnequippedHandEvent args)
    {
        _movementSpeedModifier.RefreshMovementModifiers(args.User);
    }

    public (float, float) GetHeldMovementSpeedModifiers(EntityUid uid, HeldSpeedModifierComponent component, EntityUid? owner = null) // PARADISE EDIT - Physical parameters
    {
        var walkMod = component.WalkModifier;
        var sprintMod = component.SprintModifier;

        ClothingSpeedModifierComponent? clothingSpeedModComp = null; // PARADISE EDIT START - Physical parameters

        if (component.MirrorClothingModifier && TryComp<ClothingSpeedModifierComponent>(uid, out var clothingSpeedModifier))
        {
            walkMod = clothingSpeedModifier.WalkModifier;
            sprintMod = clothingSpeedModifier.SprintModifier;
        // PARADISE EDIT START - Physical parameters
            clothingSpeedModComp = clothingSpeedModifier;
        }

        if ((component.AffectedByParameters || (clothingSpeedModComp != null && clothingSpeedModComp.AffectedByParameters)) &&
            TryComp<ItemExtensionComponent>(uid, out var itemExtensionComp) &&
            owner is { Valid: true } ownerValidated &&
            TryComp<PhysicalParametersComponent>(ownerValidated, out var parametersComp))
        {
            float parameterMultiplier = 1f;

            var ownerParameter = _parameters.GetParameterValue((ownerValidated, parametersComp), Parameter.Strength, armStrengthCounted: false);

            if (itemExtensionComp.StrengthRequirementToBeUsed != itemExtensionComp.MinimalStrengthToPickUp)
                parameterMultiplier = FixedPoint2.Clamp(1 - (ownerParameter - itemExtensionComp.MinimalStrengthToPickUp) / (itemExtensionComp.StrengthRequirementToBeUsed - itemExtensionComp.MinimalStrengthToPickUp), FixedPoint2.Zero, 1).Float();

            walkMod = 1 - (1 - walkMod) * parameterMultiplier;
            sprintMod = 1 - (1 - sprintMod) * parameterMultiplier;
        }
        // PARADISE EDIT END

        return (walkMod, sprintMod);
    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, HeldSpeedModifierComponent component, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args) // PARADISE EDIT - Physical parameters
    {
        var (walkMod, sprintMod) = GetHeldMovementSpeedModifiers(uid, component, args.Owner);
        args.Args.ModifySpeed(walkMod, sprintMod);
    }

    private void OnRefreshWeightlessModifiers(Entity<HeldSpeedModifierComponent> ent, ref HeldRelayedEvent<RefreshWeightlessModifiersEvent> args)
    {
        args.Args.ModifyAcceleration(ent.Comp.WeightlessAcceleration);
        args.Args.WeightlessModifierMod *= ent.Comp.ZeroGravityModifier;
    }
}
