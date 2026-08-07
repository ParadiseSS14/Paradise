using System.Numerics;
using Content.Client.Resources;
using Content.Shared.Wires;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client.Wires.UI;

public sealed class WireControl : Control
{
    private IResourceCache _resourceCache;

    private const string TextureContact = "/Textures/Interface/WireHacking/contact.svg.96dpi.png";

    public event Action? WireClicked;
    public event Action? ContactsClicked;

    public WireControl(WireColor color,
        WireLetter letter,
        bool isCut,
        bool flip,
        bool mirror,
        int type,
        IResourceCache resourceCache)
    {
        _resourceCache = resourceCache;

        HorizontalAlignment = HAlignment.Center;
        MouseFilter = MouseFilterMode.Stop;

        var layout = new LayoutContainer();
        AddChild(layout);

        var greek = new Label
        {
            Text = letter.Letter().ToString(),
            VerticalAlignment = VAlignment.Center,
            HorizontalAlignment = HAlignment.Center,
            Align = Label.AlignMode.Center,
            FontOverride = _resourceCache.GetFont("/Fonts/NotoSansDisplay/NotoSansDisplay-Bold.ttf", 12),
            FontColorOverride = Color.Gray,
            ToolTip = letter.Name(),
            MouseFilter = MouseFilterMode.Stop
        };

        layout.AddChild(greek);

        var contactTexture = _resourceCache.GetTexture(TextureContact);
        var contact1 = new TextureRect
        {
            Texture = contactTexture,
            Modulate = Color.FromHex("#E1CA76")
        };

        layout.AddChild(contact1);
        LayoutContainer.SetPosition(contact1, new Vector2(0, 0));

        var contact2 = new TextureRect
        {
            Texture = contactTexture,
            Modulate = Color.FromHex("#E1CA76")
        };

        layout.AddChild(contact2);
        LayoutContainer.SetPosition(contact2, new Vector2(0, 60));

        var wire = new WireRender(color, isCut, flip, mirror, type, _resourceCache);

        layout.AddChild(wire);
        LayoutContainer.SetPosition(wire, new Vector2(2, 16));

        ToolTip = color.Name();
        MinSize = new Vector2(20, 102);
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
        {
            return;
        }

        if (args.RelativePosition.Y > 20 && args.RelativePosition.Y < 60)
        {
            WireClicked?.Invoke();
        }
        else
        {
            ContactsClicked?.Invoke();
        }
    }

    protected override bool HasPoint(Vector2 point)
    {
        return base.HasPoint(point) && point.Y <= 80;
    }

    private sealed class WireRender : Control
    {
        private readonly WireColor _color;
        private readonly bool _isCut;
        private readonly bool _flip;
        private readonly bool _mirror;
        private readonly int _type;

        private static readonly string[] TextureNormal =
        {
            "/Textures/Interface/WireHacking/wire_1.svg.96dpi.png",
            "/Textures/Interface/WireHacking/wire_2.svg.96dpi.png"
        };

        private static readonly string[] TextureCut =
        {
            "/Textures/Interface/WireHacking/wire_1_cut.svg.96dpi.png",
            "/Textures/Interface/WireHacking/wire_2_cut.svg.96dpi.png",
        };

        private static readonly string[] TextureCopper =
        {
            "/Textures/Interface/WireHacking/wire_1_copper.svg.96dpi.png",
            "/Textures/Interface/WireHacking/wire_2_copper.svg.96dpi.png"
        };

        private readonly IResourceCache _resourceCache;

        public WireRender(WireColor color,
            bool isCut,
            bool flip,
            bool mirror,
            int type,
            IResourceCache resourceCache)
        {
            _resourceCache = resourceCache;
            _color = color;
            _isCut = isCut;
            _flip = flip;
            _mirror = mirror;
            _type = type;

            SetSize = new Vector2(16, 50);
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            var colorValue = _color.ColorValue();
            var tex = _resourceCache.GetTexture(_isCut ? TextureCut[_type] : TextureNormal[_type]);

            var l = 0f;
            var r = tex.Width + l;
            var t = 0f;
            var b = tex.Height + t;

            if (_flip)
            {
                (t, b) = (b, t);
            }

            if (_mirror)
            {
                (l, r) = (r, l);
            }

            l *= UIScale;
            r *= UIScale;
            t *= UIScale;
            b *= UIScale;

            var rect = new UIBox2(l, t, r, b);
            if (_isCut)
            {
                var copper = Color.Orange;
                var copperTex = _resourceCache.GetTexture(TextureCopper[_type]);
                handle.DrawTextureRect(copperTex, rect, copper);
            }

            handle.DrawTextureRect(tex, rect, colorValue);
        }
    }
}
