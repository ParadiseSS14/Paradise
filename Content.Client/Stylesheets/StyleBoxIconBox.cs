using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Content.Client.Stylesheets.Colorspace;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client.Stylesheets;

// ReSharper disable once InconsistentNaming
public sealed partial class StyleBoxIconBox : StyleBox
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IResourceCache _cache = default!;

    public Color HighlightColor { get; set; }
    public Color UnhighlitColor { get; set; }
    public Color BackgroundColor { get; set; }

    public float BorderThickness { get; set; }

    public float CornerRadius { get; set; } = 0f;

    private ShaderInstance? _shaderInstance;

    private bool _isHovered;

    public void SetHovered(bool toHover)
    {
        _isHovered = toHover;
    }

    public ProtoId<ShaderPrototype>? Shader
    {
        set => _shaderInstance = _prototype.TryIndex(value, out var proto)
            ? proto.InstanceUnique()
            : null;
    }

    private string? _texturePath;

    private Texture? _textureRef;

    private readonly ProtoId<ShaderPrototype> _IconBoxShader = "UIShaderIconBox";

    private int CornerDetail { get; set; } = 8;


    private void Init()
    {
        IoCManager.InjectDependencies(this);
        Shader = _IconBoxShader;
    }

    public StyleBoxIconBox()
    {
        Init();
    }

    public StyleBoxIconBox(Texture texture)
    {
        _textureRef = texture;
        Init();
    }

    protected override void DoDraw(DrawingHandleScreen handle, UIBox2 box, float uiScale)
    {
        Shader = _IconBoxShader; // Fixes a shared data between instances bug, but I'm uncomfortable with it too.

        handle.UseShader(_shaderInstance);
        if(_textureRef == null) return;
        _shaderInstance?.SetParameter("size", new Vector2(box.Width, box.Height));
        _shaderInstance?.SetParameter("highlightColor", HighlightColor);
        _shaderInstance?.SetParameter("unhighlitColor", UnhighlitColor);
        _shaderInstance?.SetParameter("backgroundColor", BackgroundColor.NudgeLightness(0.5f));
        _shaderInstance?.SetParameter("iconTex", _textureRef);
        _shaderInstance?.SetParameter("highlightProgress", _isHovered ? 1.0f : 0.0f);
        handle.DrawRect(box, Color.White);
        handle.UseShader(null);

    }


    protected override float GetDefaultContentMargin(Margin margin)
    {
        var t = BorderThickness;

        return margin switch
        {
            Margin.Top => t,
            Margin.Bottom => t,
            Margin.Left => t,
            Margin.Right => t,
            _ => 0f
        };
    }
}
