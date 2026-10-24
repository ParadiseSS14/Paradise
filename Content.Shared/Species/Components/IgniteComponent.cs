using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Species.Components;

/// <summary>
/// Grants an action that spits a lit flame into the user's hand, provided their mouth
/// is uncovered and they have drunk enough welding fuel. Unathi "Ignite".
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IgniteComponent : Component
{
    /// <summary>
    /// The action granted on map init.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId ActionPrototype;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// What to spawn and light. Needs a MatchstickComponent to actually catch fire.
    /// Defaults to the variant that deletes itself on burnout rather than leaving litter.
    /// </summary>
    [DataField]
    public EntProtoId FlamePrototype = "MatchstickUnathi";

    /// <summary>
    /// Message shown to the user on a successful ignite.
    /// </summary>
    [DataField(required: true)]
    public LocId PopupText;

    /// <summary>
    /// Reagent that has to be in the user before they can ignite.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> RequiredFuel = "WeldingFuel";

    /// <summary>
    /// How many units of <see cref="RequiredFuel"/> must be present to ignite.
    /// </summary>
    [DataField]
    public float RequiredFuelUnits = 3f;
}
