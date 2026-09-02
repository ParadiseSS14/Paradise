using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Paradise.Mech.Equipment.Components;

/// <summary>
/// A piece of equipment that can be installed into <see cref="AltMechComponent"/>
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PowerConsumingJetpackComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public float PowerConsumption = 3f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField]
    public TimeSpan NextPowerDrain = TimeSpan.Zero;
}
