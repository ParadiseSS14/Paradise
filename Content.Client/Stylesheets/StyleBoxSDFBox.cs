using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client.Stylesheets;

// ReSharper disable once InconsistentNaming
public sealed partial class StyleBoxSDFBox : StyleBox
{
    [Dependency] private IPrototypeManager _prototype = default!;

    public Color BackgroundColor { get; set; }
    public Color GradientBottomColor { get; set; }
    public Color BorderColor { get; set; }

    public float BorderThickness { get; set; }

    public Vector4 CornerRadius { get; set; } = new(0f, 0f, 0f, 0f);
    public bool DoGradient { get; set; } = false;

    private ShaderInstance? _shaderInstance;
    public ProtoId<ShaderPrototype>? Shader
    {
        set => _shaderInstance = _prototype.TryIndex(value, out var proto)
            ? proto.InstanceUnique()
            : null;
    }

    private readonly ProtoId<ShaderPrototype> _UISDFShaderName = "UIShaderSDF";

    private int CornerDetail { get; set; } = 8;

    private void Init()
    {
        IoCManager.InjectDependencies(this);
        Shader = _UISDFShaderName;
    }

    public StyleBoxSDFBox()
    {
        Init();
    }

    public StyleBoxSDFBox(Color backgroundColor)
    {
        Init();
        BackgroundColor = backgroundColor;
    }
    public StyleBoxSDFBox(Color gradientTopColor, Color gradientBotColor)
    {
        Init();
        DoGradient = true;
        BackgroundColor = gradientTopColor;
        GradientBottomColor = gradientBotColor;
    }

    public StyleBoxSDFBox(StyleBoxSDFBox other)
        : base(other)
    {
        Init();

        BackgroundColor = other.BackgroundColor;
        BorderColor = other.BorderColor;
        BorderThickness = other.BorderThickness;
        CornerRadius = other.CornerRadius;
        CornerDetail = other.CornerDetail;
    }

    protected override void DoDraw(DrawingHandleScreen handle, UIBox2 box, float uiScale)
    {
        Shader = _UISDFShaderName; // Fixes a shared data between instances bug, but I'm uncomfortable with it too.

        handle.UseShader(_shaderInstance);
        _shaderInstance?.SetParameter("size", new Vector2(box.Width, box.Height));
        _shaderInstance?.SetParameter("radius", CornerRadius);
        _shaderInstance?.SetParameter("color", BackgroundColor);
        _shaderInstance?.SetParameter("borderColor", BorderColor);
        _shaderInstance?.SetParameter("borderWidth", BorderThickness);
        _shaderInstance?.SetParameter("doGradient", DoGradient);
        if (DoGradient)
        {
            _shaderInstance?.SetParameter("gradientBottomColor", GradientBottomColor);
        }

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
