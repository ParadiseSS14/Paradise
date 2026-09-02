using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.Damageable;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class DamageTypeRestrictionComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<DamageContainerPrototype>? DamageContainer;
}
