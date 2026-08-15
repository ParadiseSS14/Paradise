

using System;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
namespace Content.Client.UIControls
{
    [Virtual]
    public class ContainerForkedButton : BaseButton
    {
        public const string StylePropertyStyleBox = "stylebox";
        public const string StyleClassButton = "button";
        public const string StylePseudoClassNormal = "normal";
        public const string StylePseudoClassPressed = "pressed";
        public const string StylePseudoClassHover = "hover";
        public const string StylePseudoClassDisabled = "disabled";

        public StyleBox? StyleBoxOverride { get; set; }

        protected event Action? StyleBoxOverridden;

        public StyleBox GetStyleBox()
        {
            return _actualStyleBox;
        }

        public ContainerForkedButton()
        {
            DrawModeChanged();
            _actualStyleBox = new StyleBoxFlat();
        }

        private StyleBox _actualStyleBox;

        private void SetStyleBox(StyleBox box)
        {
            _actualStyleBox = box;
            StyleBoxOverridden?.Invoke();
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var boxSize = _actualStyleBox.MinimumSize;
            var childBox = Vector2.Max(availableSize - boxSize, Vector2.Zero);
            var min = Vector2.Zero;
            foreach (var child in Children)
            {
                child.Measure(childBox);
                min = Vector2.Max(min, child.DesiredSize);
            }

            return min + boxSize;
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            var box = UIBox2.FromDimensions(Vector2.Zero, finalSize);
            var contentBox = _actualStyleBox.GetContentBox(box, 1);

            foreach (var child in Children)
            {
                child.Arrange(contentBox);
            }

            return finalSize;
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            var style = _actualStyleBox;
            var drawBox = PixelSizeBox;
            style.Draw(handle, drawBox, UIScale);
        }

        protected override void DrawModeChanged()
        {
            switch (DrawMode)
            {
                case DrawModeEnum.Normal:
                    SetOnlyStylePseudoClass(StylePseudoClassNormal);
                    break;
                case DrawModeEnum.Pressed:
                    SetOnlyStylePseudoClass(StylePseudoClassPressed);
                    break;
                case DrawModeEnum.Hover:
                    SetOnlyStylePseudoClass(StylePseudoClassHover);
                    break;
                case DrawModeEnum.Disabled:
                    SetOnlyStylePseudoClass(StylePseudoClassDisabled);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StylePropertiesChanged()
        {
            base.StylePropertiesChanged();
            if (StyleBoxOverride != null)
            {
                SetStyleBox(StyleBoxOverride);
            }

            if (TryGetStyleProperty<StyleBox>(StylePropertyStyleBox, out var box))
            {
                SetStyleBox(box);
                return;
            }
        }
    }
}
