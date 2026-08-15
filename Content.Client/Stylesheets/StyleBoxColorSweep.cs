using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client.Stylesheets;

// ReSharper disable once InconsistentNaming
public sealed partial class StyleBoxColorSweep : StyleBox
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IResourceCache _cache = default!;

    public float Val { get; set; }
    public int Mode;

    private ShaderInstance? _shaderInstance;

    public ShaderInstance? GetShaderInstance()
    {
        return _shaderInstance;
    }

    public ProtoId<ShaderPrototype>? ShaderID
    {
        set => _shaderInstance = _prototype.TryIndex(value, out var proto)
            ? proto.InstanceUnique()
            : null;
    }

    private readonly ProtoId<ShaderPrototype> _UISDFShaderName = "UIColorSweep";

    private void Init()
    {
        IoCManager.InjectDependencies(this);
        ShaderID = _UISDFShaderName;
    }

    public StyleBoxColorSweep(int mode)
    {
        Init();
        Mode = mode;
    }

    public StyleBoxColorSweep(float hue)
    {
        Init();
        Val = hue;
    }


    protected override void DoDraw(DrawingHandleScreen handle, UIBox2 box, float uiScale)
    {
        ShaderID = _UISDFShaderName; // Fixes a shared data between instances bug, but I'm uncomfortable with it too.

        handle.UseShader(_shaderInstance);
        _shaderInstance?.SetParameter("size", new Vector2(box.Width, box.Height));
        _shaderInstance?.SetParameter("val", Val);
        _shaderInstance?.SetParameter("lockedAxis", Mode);
        handle.DrawRect(box, Color.White);
        handle.UseShader(null);

    }
}
