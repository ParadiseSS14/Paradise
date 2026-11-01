using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Content.Client.Stylesheets.Colorspace;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Stylesheets;

// ReSharper disable once InconsistentNaming
public sealed partial class StyleBoxIconBox : StyleBox
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IGameTiming _timing = default!;

    public Color HighlightColor { get; set; }
    public Color UnhighlitColor { get; set; }
    public Color BackgroundColor { get; set; }

    public float BorderThickness { get; set; }

    private FrameShaderInstancePool<DrawParams>? _shaders;

    private readonly record struct DrawParams(
        Vector2 Size,
        Color HighlightColor,
        Color UnhighlitColor,
        Color BackgroundColor,
        Texture IconTex,
        float HighlightProgress);

    private bool _isHovered;

    public void SetHovered(bool toHover)
    {
        _isHovered = toHover;
    }

    public ProtoId<ShaderPrototype>? Shader
    {
        set => _shaders = _prototype.TryIndex(value, out var proto)
            ? new FrameShaderInstancePool<DrawParams>(proto)
            : null;
    }

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
        if (_textureRef == null)
            return;

        if (_shaders != null)
        {
            var p = new DrawParams(
                new Vector2(box.Width, box.Height),
                HighlightColor,
                UnhighlitColor,
                BackgroundColor,
                _textureRef,
                _isHovered ? 1.0f : 0.0f);

            if (_shaders.Next(_timing.CurFrame, p, out var shader))
            {
                shader.SetParameter("size", p.Size);
                shader.SetParameter("highlightColor", p.HighlightColor);
                shader.SetParameter("unhighlitColor", p.UnhighlitColor);
                shader.SetParameter("backgroundColor", p.BackgroundColor.NudgeLightness(0.5f));
                shader.SetParameter("iconTex", p.IconTex);
                shader.SetParameter("highlightProgress", p.HighlightProgress);
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
