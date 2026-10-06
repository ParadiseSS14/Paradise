using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Species.Components;

/// <summary>
/// Grants an action that lashes everything adjacent with the user's tail, spinning them
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class TailwhipComponent : Component
{
    /// <summary>
    /// The action granted on map init.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId ActionPrototype;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// Damage dealt to each adjacent target.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = { ["Blunt"] = 5 },
    };

    /// <summary>
    /// How far the lash reaches in tiles
    /// </summary>
    [DataField]
    public float Range = 1.5f;

    /// <summary>
    /// Stamina damage the user takes for every target hit.
    /// </summary>
    [DataField]
    public float StaminaCostPerHit = 15f;

    /// <summary>
    /// The user cannot start a lash while carrying at least this much stamina damage.
    /// </summary>
    [DataField]
    public float StaminaToStart = 50f;

    /// <summary>
    /// If the user is carrying at least this much stamina damage once the lash resolves,
    /// they are told they have run out of momentum.
    /// </summary>
    [DataField]
    public float StaminaToContinue = 60f;

    /// <summary>
    /// How long the user spins on the spot after lashing.
    /// </summary>
    [DataField]
    public TimeSpan SpinDuration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Spin rate in radians per second.
    /// </summary>
    [DataField]
    public float SpinSpeed = MathF.Tau * 2f;

    [DataField]
    public SoundSpecifier? WhipSound = new SoundPathSpecifier("/Audio/Weapons/slash.ogg")
    {
        Params = AudioParams.Default.WithVolume(-4f),
    };

    /// <summary>
    /// Chance, per lash that connects, for a cuffed user to lose their balance and go down.
    /// </summary>
    [DataField]
    public float RestrainedKnockdownChance = 0.5f;

    /// <summary>
    /// How long a user knocked down by <see cref="RestrainedKnockdownChance"/> stays down.
    /// </summary>
    [DataField]
    public TimeSpan RestrainedKnockdownDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// When the current spin ends. Null when not spinning.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? SpinEndTime;

    /// <summary>
    /// message shown to the user when the lash connects.
    /// </summary>
    [DataField(required: true)]
    public LocId PopupText;
}
