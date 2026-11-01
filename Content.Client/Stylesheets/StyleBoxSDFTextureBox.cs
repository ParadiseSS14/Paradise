using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Stylesheets;

// ReSharper disable once InconsistentNaming
public sealed partial class StyleBoxSDFTextureBox : StyleBox
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IGameTiming _timing = default!;


    public Texture? Texture { get; set; }
    public Color Modulate { get; set; } = Color.White;

    public float UiTiling { get; set; } = 1.0f;

    public Vector4 CornerRadius { get; set; } = new(0f, 0f, 0f, 0f);

    private FrameShaderInstancePool<DrawParams>? _shaders;
    public ProtoId<ShaderPrototype>? Shader
    {
        set => _shaders = _prototype.TryIndex(value, out var proto)
            ? new FrameShaderInstancePool<DrawParams>(proto)
            : null;
    }

    private readonly record struct DrawParams(
        Vector2 Size,
        float Tiling,
        Vector4 Radius,
        Texture? Texture,
        Color Modulate);

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
        if (_shaders != null)
        {
            var p = new DrawParams(new Vector2(box.Width, box.Height), UiTiling, CornerRadius, Texture, Modulate);

            if (_shaders.Next(_timing.CurFrame, p, out var shader))
            {
                shader.SetParameter("size", p.Size);
                shader.SetParameter("tiling", p.Tiling);
                shader.SetParameter("radius", p.Radius);
                if (p.Texture != null)
                {
                    shader.SetParameter("mainTex", p.Texture);
                    shader.SetParameter("texSize", new Vector2(p.Texture.Width, p.Texture.Height));
                }
                shader.SetParameter("colMod", p.Modulate);
            }

            handle.UseShader(shader);
        }

        handle.DrawRect(box, Color.White);
        handle.UseShader(null);

    }
}
