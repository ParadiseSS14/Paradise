using Content.Shared._Paradise.AltArmor;
using Content.Shared._Paradise.AltArmor.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;

namespace Content.Shared._Paradise.WearableAltArmor;

public sealed partial class WearableAltArmorSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private AltArmorSystem _altArmor = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WearableAltArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);

        SubscribeLocalEvent<WearableAltArmorComponent, DamageModifyEvent>(OnDamageModifyDirect);
    }

    public void OnDamageModify(Entity<WearableAltArmorComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        _altArmor.ModifyDamage(ent.Owner, args.Args.OriginalDamage, out var resultDamage, out var resultArmorDamage);

        _damageable.TryChangeDamage(ent.Owner, resultArmorDamage);

        args.Args.Damage = resultDamage;
    }

    public void OnDamageModifyDirect(Entity<WearableAltArmorComponent> ent, ref DamageModifyEvent args)
    {
        _altArmor.ModifyDamage(ent.Owner, args.OriginalDamage, out var resultDamage, out args.Damage);
    }
}
