using Content.Client.UserInterface.Tweens.Behaviours;
using Content.Client.UserInterface.Tweens.Delegates;
using Content.Client.UserInterface.Tweens.Tweeners;

namespace Content.Client.UserInterface.Tweens.Extensions;

public static class TweenExtensions
{
    public static TweenInstance Tween(
        Tweener<float>.Getter getter,
        Tweener<float>.Setter setter,
        Tweener<float>.Getter to,
        float duration,
        ValidationDelegates.Validation validation
    )
    {
        InterpolationTweenBehaviour tweenBehaviour = new InterpolationTweenBehaviour();
        tweenBehaviour.Add(new FloatTweener(getter, setter, to, duration, validation));
        return new TweenInstance(tweenBehaviour);
    }

    public static TweenInstance Tween(
        Tweener<float>.Getter getter,
        Tweener<float>.Setter setter,
        float to,
        float duration,
        ValidationDelegates.Validation validation
    ) => Tween(getter, setter, () => to, duration, validation);

    public static TweenInstance Tween(
        Tweener<float>.Getter getter,
        Tweener<float>.Setter setter,
        float to,
        float duration
    ) => Tween(getter, setter, to, duration, ValidationExtensions.AlwaysValid);

    public static TweenInstance Tween(
        float from,
        float to,
        Tweener<float>.Setter setter,
        float duration
    )
    {
        return Tween(
            () => from,
            setter,
            to,
            duration,
            ValidationExtensions.AlwaysValid
        );
    }

    public static bool IsPlayingOrCompleted(this TweenInstance tweenInstance)
    {
        return tweenInstance.IsPlaying || tweenInstance.IsCompleted;
    }

    public static bool IsPlayingOrCompletedOrNested(this TweenInstance tweenInstance)
    {
        return tweenInstance.IsPlaying || tweenInstance.IsCompleted || tweenInstance.IsNested;
    }

    #region Color

    public static TweenInstance Tween(
        Tweener<Color>.Getter getter,
        Tweener<Color>.Setter setter,
        Tweener<Color>.Getter to,
        float duration,
        ValidationDelegates.Validation validation
    )
    {
        InterpolationTweenBehaviour tweenBehaviour = new InterpolationTweenBehaviour();
        tweenBehaviour.Add(new ColorTweener(getter, setter, to, duration, validation));
        return new TweenInstance(tweenBehaviour);
    }

    public static TweenInstance Tween(
        Tweener<Color>.Getter getter,
        Tweener<Color>.Setter setter,
        Color to,
        float duration,
        ValidationDelegates.Validation validation
    ) => Tween(getter, setter, () => to, duration, validation);

    public static TweenInstance Tween(
        Tweener<Color>.Getter getter,
        Tweener<Color>.Setter setter,
        Color to,
        float duration
    ) => Tween(getter, setter, to, duration, ValidationExtensions.AlwaysValid);

    public static TweenInstance Tween(
        Color from,
        Color to,
        Tweener<Color>.Setter setter,
        float duration
    )
    {
        return Tween(
            () => from,
            setter,
            to,
            duration,
            ValidationExtensions.AlwaysValid
        );
    }

    #endregion
}
