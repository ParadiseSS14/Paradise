using Content.Shared.Audio;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cleaning;

/// <summary>
/// This is used for cleaning objects. Decals, fibers, anything. Extend it when you need to clean something new.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedCleaningSystem))]
public sealed partial class CleaningComponent : Component
{
    [DataField]
    public EntProtoId CleanedEffect = "PuddleSparkle";
    [DataField]
    public CleaningType CleaningStrength = CleaningType.Wash;
    [DataField]
    public TimeSpan CleaningDuration = TimeSpan.FromSeconds(3f);
    [DataField]
    public bool RequiresCleaningSolution = false;
    [DataField]
    public FixedPoint2 CleaningSolutionRequiredAmount = 5f;
    [DataField]
    public SoundSpecifier CleanSound = new SoundPathSpecifier("/Audio/Effects/Fluids/watersplash.ogg",
        AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation));

    [DataField]
    public bool PlaysSound = true;
}
