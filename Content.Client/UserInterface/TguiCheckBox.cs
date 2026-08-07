using Content.Client.UserInterface.Tweens;
using Content.Client.UserInterface.Tweens.Easers;
using Content.Client.UserInterface.Tweens.Extensions;
using Robust.Client.UserInterface.Controls;
using static Robust.Client.UserInterface.Controls.Label;

namespace Content.Client.UserInterface
{
    /// <summary>
    ///     A type of toggleable button that also has a checkbox.
    /// </summary>
    [Virtual]
    public partial class TguiCheckBox : ContainerButton
    {
        public const string StyleClassCheckBox = "checkBox";
        public const string StyleClassCheckBoxChecked = "checkBoxChecked";
        public const string StyleClassCheckedColor = "checked-color";
        public const string StyleClassUncheckedColor = "unchecked-color";

        [Dependency] private IEntityManager _entityManager = default!;

        public Label Label { get; }
        public TextureRect TextureRect { get; }

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

        /// <summary>
        /// Should the checkbox be to the left or the right of the label.
        /// </summary>
        public bool LeftAlign
        {
            get => _leftAlign;
            set
            {
                if (_leftAlign == value)
                    return;

                _leftAlign = value;

                if (value)
                {
                    Label.HorizontalExpand = false;
                    TextureRect.SetPositionFirst();
                    Label.SetPositionInParent(1);
                }
                else
                {
                    Label.HorizontalExpand = true;
                    Label.SetPositionFirst();
                    TextureRect.SetPositionInParent(1);
                }
            }
        }

        private bool _leftAlign = true;
        private readonly TweenManager? _tweenManager;

        public TguiCheckBox()
        {
            IoCManager.InjectDependencies(this);

            ToggleMode = true;

            var hBox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                StyleClasses = { StyleClassCheckBox },
            };
            AddChild(hBox);

            TextureRect = new TextureRect
            {
                StyleClasses = { StyleClassCheckBox },
                VerticalAlignment = VAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
            };

            Label = new Label();

            if (LeftAlign)
            {
                Label.HorizontalExpand = false;
                hBox.AddChild(TextureRect);
                hBox.AddChild(Label);
            }
            else
            {
                Label.HorizontalExpand = true;
                hBox.AddChild(Label);
                hBox.AddChild(TextureRect);
            }

            if (_entityManager != null && _entityManager.TrySystem<TweenManager>(out var tweenManager))
            {
                _tweenManager = tweenManager;
            }
        }

        private bool _oldCheckedStatus;

        protected override void DrawModeChanged()
        {
            base.DrawModeChanged();

            if (TextureRect != null)
            {
                if (Pressed)
                {
                    TextureRect.AddStyleClass(StyleClassCheckBoxChecked);
                    // should we add this to the stylepsuedoclasses? IDK. can't do it myself.
                    AddStyleClass("checked");
                    if (Pressed != _oldCheckedStatus)
                        TryTween(UncheckedColor, CheckedColor);
                }
                else
                {
                    TextureRect.RemoveStyleClass(StyleClassCheckBoxChecked);
                    RemoveStyleClass("checked");
                    if (Pressed != _oldCheckedStatus)
                        TryTween(CheckedColor, UncheckedColor);
                }
            }

            _oldCheckedStatus = Pressed;
        }

        private void TryTween(Color oldColor, Color newColor)
        {
            var tween = TweenExtensions.Tween(
                    oldColor,
                    newColor,
                    v => TextureRect.ModulateSelfOverride = v,
                    0.15f)
                .SetEasing(Easing.InOutQuint);

            if (!tween.IsPlaying)
            {
                _tweenManager?.Play(tween);
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
