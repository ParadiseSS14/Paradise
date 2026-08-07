using Content.Client.UserInterface.Tweens.Delegates;
using Content.Client.UserInterface.Tweens.Interpolators;

namespace Content.Client.UserInterface.Tweens.Tweeners;

public sealed class FloatTweener : Tweener<float>
{
    public FloatTweener(
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
            FloatInterpolator.Instance,
            validation
        )
    {
    }
}
