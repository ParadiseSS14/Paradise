using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client.Stylesheets;

// ReSharper disable once InconsistentNaming
public sealed class StyleBoxSDFTextureBox : StyleBox
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    public Texture? Texture { get; set; }
    public Color Modulate { get; set; } = Color.White;

    public float UiTiling { get; set; } = 1.0f;

    public Vector4 CornerRadius { get; set; } = new(0f, 0f, 0f, 0f);
    public bool DoGradient { get; set; } = false;

    private ShaderInstance? _shaderInstance;
    public ProtoId<ShaderPrototype>? Shader
    {
        set => _shaderInstance = _prototype.TryIndex(value, out var proto)
            ? proto.InstanceUnique()
            : null;
    }

    private readonly ProtoId<ShaderPrototype> _UISDFShaderName = "UIShaderSDFTexture";

    private int CornerDetail { get; set; } = 8;

    private void Init()
    {
        IoCManager.InjectDependencies(this);
        Shader = _UISDFShaderName;
    }

    public StyleBoxSDFTextureBox()
    {
        Init();
    }

    public StyleBoxSDFTextureBox(Texture texture)
    {
        Init();
        Texture = texture;
    }

    public StyleBoxSDFTextureBox(StyleBoxSDFTextureBox other)
        : base(other)
    {
        Init();
        Texture = other.Texture;
        CornerRadius = other.CornerRadius;
        CornerDetail = other.CornerDetail;
        Modulate = other.Modulate;
    }

    protected override void DoDraw(DrawingHandleScreen handle, UIBox2 box, float uiScale)
    {
        Shader = _UISDFShaderName; // Fixes a shared data between instances bug, but I'm uncomfortable with it too.

        handle.UseShader(_shaderInstance);
        _shaderInstance?.SetParameter("size", new Vector2(box.Width, box.Height));
        _shaderInstance?.SetParameter("tiling", UiTiling);
        _shaderInstance?.SetParameter("radius", CornerRadius);
        if (Texture != null)
        {
            _shaderInstance?.SetParameter("mainTex", Texture);
            _shaderInstance?.SetParameter("texSize", new Vector2(Texture.Width, Texture.Height));
        }
        _shaderInstance?.SetParameter("colMod", Modulate);
        handle.DrawRect(box, Color.White);
        handle.UseShader(null);

    }
}
