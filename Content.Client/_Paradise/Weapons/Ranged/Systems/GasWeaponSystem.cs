using Content.Shared._Paradise.Weapons.Components;
using Content.Shared._Paradise.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Client._Paradise.Weapons.Ranged.Systems;

public sealed partial class GasWeaponSystem : SharedGasWeaponSystem
{

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void OnShootAttempt(Entity<GasWeaponComponent> ent, ref ShotAttemptedEvent args)
    {
        base.OnShootAttempt(ent, ref args);
    }

}
