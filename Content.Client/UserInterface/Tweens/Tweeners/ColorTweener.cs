using Content.Client.UserInterface.Tweens.Delegates;
using Content.Client.UserInterface.Tweens.Interpolators;

namespace Content.Client.UserInterface.Tweens.Tweeners;

public sealed class ColorTweener : Tweener<Color>
{
    public ColorTweener(
        Getter currentValueGetter,
        Setter setter,
        Getter to,
        float duration,
        ValidationDelegates.Validation validation
    )
        : base(
            currentValueGetter,
            setter,
            to,
            duration,
            ColorInterpolator.Instance,
            validation
        )
    {
    }
}
