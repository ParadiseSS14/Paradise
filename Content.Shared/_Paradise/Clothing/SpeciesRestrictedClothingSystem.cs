using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;

namespace Content.Shared._Paradise.Clothing;

public sealed class SpeciesRestrictedClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeciesRestrictedClothingComponent, BeingEquippedAttemptEvent>(EquipAttempt);
    }

    private void EquipAttempt(Entity<SpeciesRestrictedClothingComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (!TryComp<HumanoidProfileComponent>(args.EquipTarget, out var humanoidComp))
        {
            args.Cancel();
            return;
        }

        if (ent.Comp.AllowedSpecies.Count > 0 &&
            !ent.Comp.AllowedSpecies.Contains(humanoidComp.Species))
        {
            args.Cancel();
            return;
        }

        if (ent.Comp.BannedSpecies.Count > 0 &&
            ent.Comp.BannedSpecies.Contains(humanoidComp.Species))
        {
            args.Cancel();
            return;
        }
    }
}
