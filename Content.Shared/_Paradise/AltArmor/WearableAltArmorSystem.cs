using Content.Shared._Paradise.AltArmor.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;

namespace Content.Shared._Paradise.AltArmor;

public sealed partial class WearableAltArmorSystem : AltArmorSystem<WearableAltArmorComponent>
{
    [Dependency] private DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WearableAltArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);

        SubscribeLocalEvent<WearableAltArmorComponent, DamageModifyEvent>(OnDamageModifyDirect);
    }

    public void OnDamageModify(Entity<WearableAltArmorComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        ModifyDamage(ent.Owner, args.Args.OriginalDamage, out var resultDamage, out var resultArmorDamage);

        _damageable.TryChangeDamage(ent.Owner, resultArmorDamage);

        args.Args.Damage = resultDamage;
    }

    public void OnDamageModifyDirect(Entity<WearableAltArmorComponent> ent, ref DamageModifyEvent args)
    {
        if (TryComp<ClothingComponent>(ent.Owner, out var clothing) && clothing.InSlot != null)
            return;

        ModifyDamage(ent.Owner, args.OriginalDamage, out var _, out args.Damage);
    }
}
