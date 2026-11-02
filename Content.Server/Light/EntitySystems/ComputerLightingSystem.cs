using System.Linq;
using Content.Server.Light.Components;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Colorspace;
using Content.Shared.Roles;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Light.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class ComputerLightingSystem : EntitySystem
{
    public const float Energy = 3f;
    public const float Radius = 1.6f;
    public const float Falloff = 2f;

    [Dependency] private SharedPointLightSystem _light = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;

    /// <summary>
    /// Cached versions of colors from Department Colors
    /// </summary>
    private Dictionary<string, Color> _departmentColors =  new Dictionary<string, Color>();

    private Dictionary<string, List<string>> _cachedDepartmentsFromAccess = new Dictionary<string, List<string>>();

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ComputerLightingComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnReload);

        BuildCache();
    }

    private void OnReload(PrototypesReloadedEventArgs msg, EntitySessionEventArgs args)
    {
        if (msg.WasModified<DepartmentPrototype>() || msg.WasModified<AccessGroupPrototype>() ||
            msg.WasModified<JobPrototype>())
            BuildCache();
    }

    private void BuildCache()
    {
        _cachedDepartmentsFromAccess.Clear();
        _departmentColors.Clear();

        foreach (var department in _protoMan.EnumeratePrototypes<DepartmentPrototype>())
        {
            _cachedDepartmentsFromAccess[department.ID] = [.. department.Roles.Select(r => (string)r)];
        }

        foreach (var pair in _protoMan.EnumeratePrototypes<AccessGroupPrototype>())
        {
            var departmentId = _cachedDepartmentsFromAccess
                .Where(kv => kv.Value.Any(jobId =>
                    _protoMan.Index<JobPrototype>(jobId).AccessGroups.Any(g => g == pair.ID)))
                .OrderByDescending(kv => kv.Key == "Command")
                .Select(kv => kv.Key)
                .FirstOrDefault();

            if (departmentId is null ||
                !_protoMan.TryIndex<DepartmentPrototype>(departmentId, out var department))
                continue;

            var color = department.Color.NudgeLightness(0.15f);

            _departmentColors[pair.ID] = color;
            foreach (var tag in pair.Tags)
            {
                _departmentColors.TryAdd(tag, color);
            }
        }
    }

    private void OnComponentInit(EntityUid uid, ComputerLightingComponent component, ref ComponentInit args)
    {
        EnsureComp<PointLightComponent>(uid);
        EnsureComp<LitOnPoweredComponent>(uid);

        _light.SetRadius(uid, Radius);
        _light.SetFalloff(uid, Falloff);
        _light.SetEnergy(uid, Energy);

        if (!TryComp<AccessReaderComponent>(uid, out var reader))
            return;

        foreach (var tag in reader.AccessLists.SelectMany(list => list))
        {
            if (!_departmentColors.TryGetValue(tag, out var color))
                continue;

            _light.SetColor(uid, color);
            break;
        }
    }
}
