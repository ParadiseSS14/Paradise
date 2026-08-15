using System.Numerics;
using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Controls.ColorPicker;

public sealed class HSVRGBSlider : Slider
{

    public Color Color { get; private set; } = Color.White;

    public void SetColor(Color color)
    {
        Color = color;
        _backgroundPanel.Modulate = Color;
    }

    protected override void UpdateStyleBoxes()
    {
        StyleBox? GetStyleBox(string name)
        {
            if (TryGetStyleProperty<StyleBox>(name, out var box))
            {
                return box;
            }

            return null;
        }

        string backBox = StylePropertyBackground;

        _backgroundPanel.PanelOverride = BackgroundStyleBoxOverride ?? GetStyleBox(backBox);
        _grabber.PanelOverride = GrabberStyleBoxOverride ?? GetStyleBox(StylePropertyGrabber);
    }

    public enum PartSelector
    {
        Fill,
        Background
    }

    public void SetValForSweep(Vector4 hsva, int mode)
    {
        var box = BackgroundStyleBoxOverride as StyleBoxColorSweep;

        box?.Val = hsva.X;

    }

}
