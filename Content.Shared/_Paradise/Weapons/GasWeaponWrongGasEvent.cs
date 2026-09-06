using Content.Shared._Paradise.Weapons.Components;

namespace Content.Shared._Paradise.Weapons.Ranged;

[ByRefEvent]
public record struct GasWeaponWrongGasEvent(Entity<GasWeaponComponent> ent, float amountOfMoles);
