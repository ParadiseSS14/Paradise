using System.Numerics;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Stylesheets;
using Content.Shared.Wires;
using Robust.Client.Animations;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Animations;

namespace Content.Client.Wires.UI;

public sealed class StatusLightUi : Control
{

    private static readonly Animation _blinkingFast = new()
    {
        Length = TimeSpan.FromSeconds(0.2),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = nameof(Control.Modulate),
                InterpolationMode = AnimationInterpolationMode.Linear,
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.Transparent, 0.1f),
                    new AnimationTrackProperty.KeyFrame(Color.White, 0.1f)
                }
            }
        }
    };

    private static readonly Animation _blinkingSlow = new()
    {
        Length = TimeSpan.FromSeconds(0.8),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = nameof(Control.Modulate),
                InterpolationMode = AnimationInterpolationMode.Linear,
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, 0.3f),
                    new AnimationTrackProperty.KeyFrame(Color.Transparent, 0.1f),
                    new AnimationTrackProperty.KeyFrame(Color.Transparent, 0.3f),
                    new AnimationTrackProperty.KeyFrame(Color.White, 0.1f),
                }
            }
        }
    };

    public StatusLightUi(StatusLightData data, IResourceCache resourceCache)
            {
                RobustXamlLoader.Load(this);
                HorizontalAlignment = HAlignment.Right;


                var hsv = Color.ToHsv(data.Color);
                hsv.Z /= 2;
                var dimColor = Color.FromHsv(hsv);
                TextureRect activeLight;

                var lightContainer = new Control
                {
                    SetSize = new Vector2(20, 20),
                    Children =
                    {
                        new TextureRect
                        {
                            Texture = resourceCache.GetTexture(
                                "/Textures/Interface/WireHacking/light_off_base.svg.96dpi.png"),
                            Stretch = TextureRect.StretchMode.KeepCentered,
                            ModulateSelfOverride = dimColor
                        },
                        (activeLight = new TextureRect
                        {
                            ModulateSelfOverride = data.Color.WithAlpha(0.4f),
                            Stretch = TextureRect.StretchMode.KeepCentered,
                            Texture =
                                resourceCache.GetTexture("/Textures/Interface/WireHacking/light_on_base.svg.96dpi.png"),
                        })
                    }
                };

                Animation? animation = null;

                switch (data.State)
                {
                    case StatusLightState.Off:
                        activeLight.Visible = false;
                        break;
                    case StatusLightState.On:
                        break;
                    case StatusLightState.BlinkingFast:
                        animation = _blinkingFast;
                        break;
                    case StatusLightState.BlinkingSlow:
                        animation = _blinkingSlow;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (animation != null)
                {
                    activeLight.PlayAnimation(animation, "blink");

                    activeLight.AnimationCompleted += s =>
                    {
                        if (s == "blink")
                        {
                            activeLight.PlayAnimation(animation, s);
                        }
                    };
                }

                var hBox = new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Horizontal,
                    SeparationOverride = 4
                };
                hBox.AddChild(new Label
                {
                    Text = data.Text,
                    StyleClasses = { "LabelSubText" },
                    VerticalAlignment = VAlignment.Center,
                });
                hBox.AddChild(lightContainer);
                hBox.AddChild(new Control {MinSize = new Vector2(6, 0)});
                AddChild(hBox);
            }
}
