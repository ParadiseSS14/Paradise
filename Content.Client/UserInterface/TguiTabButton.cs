using Content.Client.UserInterface.Tweens;
using Content.Client.UserInterface.Tweens.Easers;
using Content.Client.UserInterface.Tweens.Extensions;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Robust.Client.UserInterface.Controls.Label;

namespace Content.Client.UserInterface
{
    /// <summary>
    ///     A type of toggleable button that also has a checkbox.
    /// </summary>
    [Virtual]
    public partial class TguiTabButton : ContainerButton
    {
        public const string StyleClassCheckBox = "checkBox";
        public const string StyleClassCheckBoxChecked = "checkBoxChecked";
        public const string StyleClassCheckedColor = "checked-color";
        public const string StyleClassUncheckedColor = "unchecked-color";

        [Dependency] private IEntityManager _entityManager = default!;

        public Label Label { get; }

        [ViewVariables(VVAccess.ReadWrite)]
        public Color CheckedColor
        {
            get
            {
                if (TryGetStyleProperty(StyleClassCheckedColor, out Color value))
                    return value;

                return Color.White;
            }
        }

        [ViewVariables(VVAccess.ReadWrite)]
        public Color UncheckedColor
        {
            get
            {
                if (TryGetStyleProperty(StyleClassUncheckedColor, out Color value))
                    return value;

                return Color.White;
            }
        }


        private bool _leftAlign = true;
        private readonly TweenManager? _tweenManager;

        public TguiTabButton(bool  leftAlign = true)
        {
            IoCManager.InjectDependencies(this);
            _leftAlign = leftAlign;
            ToggleMode = true;



            var hBox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                StyleClasses = { StyleClassCheckBox },
            };
            AddChild(hBox);

            Label = new Label()
            {
                Margin = new Thickness(16, 8),
            };
            var spacer = new Control{Name = "spacer", HorizontalExpand = true};

            selectedRect = new PanelContainer{ModulateSelfOverride = CheckedColor, VerticalExpand = true, SetWidth = 0f, StyleClasses = { "selectionHandle" },};

            if (_leftAlign)
            {
                hBox.AddChild(selectedRect);
                hBox.AddChild(Label);
                hBox.AddChild(spacer);
            }
            else
            {
                hBox.AddChild(spacer);
                hBox.AddChild(Label);
                hBox.AddChild(selectedRect);
            }

            if (_entityManager.TrySystem<TweenManager>(out var tweenManager))
            {
                _tweenManager = tweenManager;
            }
        }

        private bool _oldCheckedStatus;
        private PanelContainer selectedRect;

        protected override void DrawModeChanged()
        {
            base.DrawModeChanged();

                if (Pressed)
                {
                    selectedRect.Visible = true;
                    AddStyleClass("checked");
                    if (Pressed != _oldCheckedStatus)
                        TryTween(UncheckedColor, CheckedColor, 0, 6);
                }
                else
                {
                    RemoveStyleClass("checked");
                    if (Pressed != _oldCheckedStatus)
                        TryTween(CheckedColor, UncheckedColor, 6, 0);
                }

            _oldCheckedStatus = Pressed;
        }

        // param stuffing my beloved
        private void TryTween(Color oldColor, Color newColor, float oldWidth, float newWidth)
        {
            var tween = TweenExtensions.Tween(
                    oldColor,
                    newColor,
                    v => selectedRect.ModulateSelfOverride = v,
                    0.15f)
                .SetEasing(Easing.InOutQuint);

            if (!tween.IsPlaying)
            {
                _tweenManager?.Play(tween);
            }

            var sizeTween = TweenExtensions.Tween(
                    oldWidth,
                    newWidth,
                    v => selectedRect.SetWidth = v,
                    0.25f)
                .SetEasing(Easing.OutQuint);

            if (!sizeTween.IsPlaying)
            {
                _tweenManager?.Play(sizeTween);
            }
        }

        /// <summary>
        ///     How to align the text inside the button.
        /// </summary>
        [ViewVariables]
        public AlignMode TextAlign { get => Label.Align; set => Label.Align = value; }

        /// <summary>
        ///     If true, the button will allow shrinking and clip text
        ///     to prevent the text from going outside the bounds of the button.
        ///     If false, the minimum size will always fit the contained text.
        /// </summary>
        [ViewVariables]
        public bool ClipText { get => Label.ClipText; set => Label.ClipText = value; }

        /// <summary>
        ///     The text displayed by the button.
        /// </summary>
        [ViewVariables]
        public string? Text { get => Label.Text; set => Label.Text = value; }
    }
}
