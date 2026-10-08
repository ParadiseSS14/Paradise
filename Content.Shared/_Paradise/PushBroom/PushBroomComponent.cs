using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.PushBroom;

/// <summary>
/// Marks an item as a push broom. While wielded, it sweeps loose items off the
/// wielder's tile into the tile ahead of them as they walk.
/// </summary>
[RegisterComponent]
public sealed partial class PushBroomComponent : Component
{
    /// <summary>
    /// Maximum entities swept in a single step. Stops someone from stalling the server with a 500-item hoard pile.
    /// </summary>
    [DataField]
    public int PushLimit = 20;

    /// <summary>
    /// How hard swept items are shoved.
    /// </summary>
    [DataField]
    public float PushSpeed = 8f;

    /// <summary>
    /// How many tiles ahead a swept item is aimed at.
    /// </summary>
    [DataField]
    public float PushDistance = 1.25f;

    /// <summary>
    /// How strongly each sweep pulls an item toward the centre of the tile it lands
    /// on, from 0 to 1. 0 shoves items straight ahead and preserves whatever sub-tile
    /// offset they already had. 1 aims them dead at the tile centre in a single sweep.
    /// Values in between converge gradually, so a scattered pile tidies itself up over
    /// several sweeps rather than snapping into line.
    /// </summary>
    [DataField]
    public float CenteringStrength = 0.6f;


    /// <summary>
    /// The rate at which the player is slowed down when sweeping items. 1 is full speed, 0 is no movement.
    /// </summary>
    [DataField]
    public float SweepSlowdownStrength = 0.33f;

    /// <summary>
    /// Tags identifying something a swept item should be dumped into rather than
    /// shoved past. Checked against loose bags on the floor and against bags sitting
    /// in a trolley's item slots.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> TrashBagTags = new() { "TrashBag", "TrashBagHolding" };

    /// <summary>
    /// Item slot on a trolley or cart that a trash bag lives in.
    /// </summary>
    [DataField]
    public string TrashBagSlot = "trashbag_slot";

    /// <summary>
    /// The sound effect played every time the broom sweeps something.
    /// </summary>
    [DataField]
    public SoundSpecifier? SweepSound = new SoundPathSpecifier(
        "/Audio/Effects/sweeping.ogg",
        AudioParams.Default.WithVolume(12f));
}
