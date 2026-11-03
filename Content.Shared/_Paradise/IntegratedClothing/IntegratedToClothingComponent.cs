using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IntegratedToClothingComponent : Component
{
    /// <summary>
    ///     The Uid of the piece of clothing that this entity belongs to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid AttachedUid;
}
