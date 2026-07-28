using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Client._DV.UserInterfaces;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Localization;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.UserInterface.Controls
{
    [Virtual]
    public class TguiKnob : Control
    {
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        public const string StylePropertyFgColor = "foregroundColor";
        public const string StylePropertyBgColor = "backgroundColor";
        public const string StylePropertyKnobColor = "knobColor";
        public const string StylePropertyKnobHighlightColor = "knobHighlightColor";
        public const string NormalBgColor = "#313138";

        public float MaxValue { get; set; } = 100;
        public float MinValue { get; set; } = 0;
        public float Value { get; set; } = 50;

        public Color BackgroundColor { get; set; }
        public Color FillColor { get; set; }

        public Color KnobColor { get; set; }
        public Color KnobHighlightColor { get; set; }

        private float _oldVal;
        private Vector2 _grabbedMousePos;

        private Texture _defaultRingTexture;
        public Texture? RingTextureOverride { get; set; }

        private readonly ProtoId<ShaderPrototype> _RadialProgressKnobShaderName = "UIShaderRadialKnobProgress";

        private ShaderInstance? _shaderInstance;
        public ProtoId<ShaderPrototype>? Shader
        {
            set => _shaderInstance = _prototype.TryIndex(value, out var proto)
                ? proto.InstanceUnique()
                : null;
        }

        public event Action<TguiKnob>? OnGrabbed;
        public event Action<TguiKnob>? OnReleased;
        public event Action<TguiKnob>? OnValueEdit;

        public bool IsGrabbed => _grabbed;

        private bool _grabbed;

        protected override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            if (args.Function != EngineKeyFunctions.UIClick)
            {
                return;
            }
            _grabbed = true;
            _oldVal = Value;
            _grabbedMousePos = args.RelativePixelPosition;
            HandlePositionChange(args.RelativePosition);
            OnGrabbed?.Invoke(this);

        }

        protected override void KeyBindUp(GUIBoundKeyEventArgs args)
        {
            base.KeyBindUp(args);

            if (args.Function != EngineKeyFunctions.UIClick || !_grabbed)
                return;

            _grabbed = false;
            OnReleased?.Invoke(this);
        }

        protected override void MouseMove(GUIMouseMoveEventArgs args)
        {
            if (!_grabbed)
            {
                return;
            }

            HandlePositionChange(args.RelativePosition);
        }

        private void HandlePositionChange(Vector2 mousePos)
        {
            var dist = mousePos.Y - _grabbedMousePos.Y;
            Value = float.Clamp(_oldVal + dist, MinValue, MaxValue);
            OnValueEdit?.Invoke(this);
        }

        public TguiKnob()
        {
            IoCManager.InjectDependencies(this);

            _defaultRingTexture = _resourceCache.GetTexture("/Textures/Interface/Nano/chevron-right.png");

            Shader = _RadialProgressKnobShaderName;
        }



        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            if (_shaderInstance == null)
                return;

            if (TryGetStyleProperty<Color>(StylePropertyBgColor, out var bgColor))
                BackgroundColor = bgColor;

            if (TryGetStyleProperty<Color>(StylePropertyFgColor, out var fgColor))
                FillColor = fgColor;

            if (TryGetStyleProperty<Color>(StylePropertyKnobColor, out var kbColor))
                KnobColor = kbColor;

            if (TryGetStyleProperty<Color>(StylePropertyKnobHighlightColor, out var kbhighlightColor))
                KnobHighlightColor = kbhighlightColor;

            MouseFilter = MouseFilterMode.Stop;
            handle.UseShader(_shaderInstance);

            _shaderInstance.SetParameter("progressNormalized",
                (Value - MinValue) / (MaxValue - MinValue));
            _shaderInstance.SetParameter("minRange", 0.1f);
            _shaderInstance.SetParameter("maxRange", 0.9f);
            _shaderInstance.SetParameter("backgroundColor", BackgroundColor);
            _shaderInstance.SetParameter("fillColor", FillColor);
            _shaderInstance.SetParameter("knobColor", KnobColor);
            _shaderInstance.SetParameter("knobHighlightColor", KnobHighlightColor);


            _shaderInstance.SetParameter("ringTex", RingTextureOverride ?? _defaultRingTexture);


            handle.DrawRect(PixelSizeBox, Color.White);

            handle.UseShader(null);
        }
    }
}
