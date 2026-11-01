using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Stylesheets;

// ReSharper disable once InconsistentNaming
public sealed partial class StyleBoxSDFBox : StyleBox
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IGameTiming _timing = default!;

    public Color BackgroundColor { get; set; }
    public Color GradientBottomColor { get; set; }
    public Color BorderColor { get; set; }

    public float BorderThickness { get; set; }

    public Vector4 CornerRadius { get; set; } = new(0f, 0f, 0f, 0f);
    public bool DoGradient { get; set; } = false;

    private FrameShaderInstancePool<DrawParams>? _shaders;
    public ProtoId<ShaderPrototype>? Shader
    {
        set => _shaders = _prototype.TryIndex(value, out var proto)
            ? new FrameShaderInstancePool<DrawParams>(proto)
            : null;
    }

    private readonly record struct DrawParams(
        Vector2 Size,
        Vector4 Radius,
        Color Color,
        Color BorderColor,
        float BorderWidth,
        bool DoGradient,
        Color GradientBottomColor);

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
        if (_shaders != null)
        {
            var p = new DrawParams(
                new Vector2(box.Width, box.Height),
                CornerRadius,
                BackgroundColor,
                BorderColor,
                BorderThickness,
                DoGradient,
                GradientBottomColor);

            if (_shaders.Next(_timing.CurFrame, p, out var shader))
            {
                shader.SetParameter("size", p.Size);
                shader.SetParameter("radius", p.Radius);
                shader.SetParameter("color", p.Color);
                shader.SetParameter("borderColor", p.BorderColor);
                shader.SetParameter("borderWidth", p.BorderWidth);
                shader.SetParameter("doGradient", p.DoGradient);
                if (p.DoGradient)
                    shader.SetParameter("gradientBottomColor", p.GradientBottomColor);
            }

            handle.UseShader(shader);
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
