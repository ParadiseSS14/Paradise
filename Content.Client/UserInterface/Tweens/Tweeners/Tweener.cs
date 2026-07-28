using Content.Client.UserInterface.Tweens.Delegates;
using Content.Client.UserInterface.Tweens.Easers;
using Content.Client.UserInterface.Tweens.Interpolators;

namespace Content.Client.UserInterface.Tweens.Tweeners;

public class Tweener<T> : ITweener
{
    public delegate void Setter(T value);

    public delegate T Getter();

    public bool IsPlaying { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsKilled { get; private set; }
    public bool IsCompletedOrKilled => IsCompleted || IsKilled;

    public float Duration { get; }
    public float Elapsed { get; private set; }
    public float Remaining => Math.Max(Duration - Elapsed, 0f);

    private readonly Getter _getter;
    private readonly Setter _setter;
    private readonly Getter _to;
    readonly ValidationDelegates.Validation _validation;

    readonly IInterpolator<T> _interpolator;

    private bool _hasFirstTimeValues;
    private T _firstTimeInitialValue = default!;
    private T _firstTimeFinalValue = default!;

    private T _initialValue = default!;
    private T _finalValue = default!;
    private T _currentValue = default!;

    EasingDelegate _easingFunction = PresetEasingDelegateFactory.GetEaseDelegate(Easing.Linear);

    public Tweener(
        Getter getter,
        Setter setter,
        Getter to,
        float duration,
        IInterpolator<T> interpolator,
        ValidationDelegates.Validation validation
    )
    {
        _getter = getter;
        _setter = setter;
        _to = to;
        _interpolator = interpolator;
        _validation = validation;

        Duration = Math.Max(duration, 0.0f);
    }

    public void Start()
    {
        if (IsPlaying)
        {
            return;
        }

        IsPlaying = true;
        IsCompleted = false;
        IsKilled = false;

        Elapsed = 0f;

        _initialValue = _getter.Invoke();

        GetFirstTimeValues();
    }

    public void Reset()
    {
        GetFirstTimeValues();

        IsCompleted = false;
        IsKilled = false;
        Elapsed = 0f;

        _setter(_firstTimeInitialValue);
        _finalValue = _firstTimeFinalValue;
    }

    public void Tick(float delta)
    {
        if (!IsPlaying)
        {
            return;
        }
        Elapsed = Math.Min(Duration, Elapsed + delta);

        if (Elapsed < Duration)
        {
            float timeNormalized = (Elapsed / Duration);

            _currentValue = _interpolator.Evaluate(
                _initialValue,
                _finalValue,
                timeNormalized,
                _easingFunction
            );

            _setter(_currentValue);
        }
        else
        {
            Complete();
        }
    }

    public void Complete()
    {
        GetFirstTimeValues();

        T newValue = _interpolator.Evaluate(_initialValue, _finalValue, 1.0f, _easingFunction);

        _setter(newValue);

        IsPlaying = false;
        IsCompleted = true;
    }

    public void Kill()
    {
        IsKilled = true;
        IsPlaying = false;
    }

    public void SetEasing(EasingDelegate easingFunction)
    {
        _easingFunction = easingFunction;
    }

    void GetFirstTimeValues()
    {
        if (_hasFirstTimeValues)
        {
            return;
        }

        _hasFirstTimeValues = true;

        _finalValue = _to.Invoke();

        _firstTimeInitialValue = _initialValue;
        _firstTimeFinalValue = _finalValue;
    }

}
