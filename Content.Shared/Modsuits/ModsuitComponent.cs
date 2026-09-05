using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Modsuits.Components;

/// <summary>
/// Component that holds modsuit specific data, such as the action entity and the action prototype ID.
/// Modular components can be added to the modsuit entity to provide additional functionality, such as deploying the modsuit or accessing the status panel.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModsuitComponent : Component
{
    /// <summary>
    ///  Some modsuits, like cargo, can be some...non-hermitic, and we must deal with it.
    /// This variables resolve this problem
    /// </summary>
    ///
    [DataField]
    public bool ProvidesPressureProtection = true;
    [DataField]
    public bool ProvidesTempretureProtection = true;
    /// <summary>
    /// The sounds paths
    /// </summary>
    [DataField]
    public SoundSpecifier DeploySound = new SoundPathSpecifier("/Audio/Mecha/mechmove03.ogg");
    [DataField]
    public SoundSpecifier ErrorSound = new SoundPathSpecifier("/Audio/Machines/airlock_deny.ogg");
    [DataField]
    public SoundSpecifier PowerOnSound = new SoundPathSpecifier("/Audio/Misc/cryo_warning.ogg");
    /// <summary>
    /// Proyo ID of actions
    /// </summary>
    [DataField]
    public List<EntProtoId> ActionEndpoints = new();
    [NonSerialized, AutoNetworkedField]
    public List<EntityUid> ActionEntities = new();
    /// <summary>
    /// Dicts for modsuit parts information
    /// </summary>
    [DataField]
    public Dictionary<ModsuitPartType, EntProtoId> Parts = new();
    [ViewVariables, AutoNetworkedField]
    public Dictionary<ModsuitPartType, bool> DeployedParts = new();
    [ViewVariables, AutoNetworkedField]
    public Dictionary<ModsuitPartType, EntityUid> SpawnedParts = new();
    [ViewVariables, AutoNetworkedField]
    public double ActivateDelay = 1.0;
    [ViewVariables, AutoNetworkedField]
    public bool PowerOn = false;
}
