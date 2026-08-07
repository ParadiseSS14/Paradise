using Content.Client.UserInterface.Tweens.Easers;

namespace Content.Client.UserInterface.Tweens.Interpolators;

public sealed class ColorInterpolator : IInterpolator<Color>
{
    public static readonly ColorInterpolator Instance = new();

    ColorInterpolator()
    {
    }

    public Color Evaluate(
        Color initialValue,
        Color finalValue,
        float time,
        EasingDelegate easingDelegate
    )
    {
        return new Color(
                easingDelegate(initialValue.R, finalValue.R, time),
                easingDelegate(initialValue.G, finalValue.G, time),
                easingDelegate(initialValue.B, finalValue.B, time),
                easingDelegate(initialValue.A, finalValue.A, time));
    }

    public Color Subtract(Color initialValue, Color finalValue)
    {
        return new Color(
            finalValue.R - initialValue.R,
            finalValue.G - initialValue.G,
            finalValue.B - initialValue.B,
            finalValue.A - initialValue.A
        );
    }

    public Color Add(Color initialValue, Color finalValue)
    {
        return new Color(
            finalValue.R + initialValue.R,
            finalValue.G + initialValue.G,
            finalValue.B + initialValue.B,
            finalValue.A + initialValue.A
        );
    }
}
