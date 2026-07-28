using Content.Client.UserInterface.Tweens.Easers;

namespace Content.Client.UserInterface.Tweens;

public sealed class TweenInstance
{
    // Majorly Taken from https://github.com/Guillemsc/GTweens/blob/main/Source/Tweens/GTween.cs
    // Tweens are hard, this is MIT, so.
    public event Action? OnStartAction;
    public event Action? OnTickAction;
    public event Action? OnLoopAction;
    public event Action? OnResetAction;
    public event Action? OnCompleteAction;
    public event Action? OnKillAction;
    public event Action? OnCompleteOrKillAction;
    public event Action<float>? OnTimeScaleChangedAction;

    public ITweenBehaviour Behaviour { get; }
    public float TimeScale { get; set; } = 1;

    public float Delay { get; private set; }

    public bool IsNested { get; set; }
    public bool IsPlaying { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsKilled { get; private set; }
    public bool IsAlive { get; set; }
    public bool IsCompletedOrKilled => IsCompleted || IsKilled;

    private float _delayRemaining;

    public TweenInstance(ITweenBehaviour behaviour)
    {
        Behaviour = behaviour;
    }

    /// <summary>
    /// Starts the tween.
    /// </summary>
    /// <param name="isCompletingInstantly">Determines if the tween that's being started, should also complete instantly.</param>
    public void Start(bool isCompletingInstantly = false)
    {
        if (IsPlaying)
        {
            Kill();
        }

        IsPlaying = true;
        IsCompleted = false;
        IsKilled = false;


        _delayRemaining = Delay;

        Behaviour.Start(isCompletingInstantly);

        OnStartAction?.Invoke();
    }

    /// <summary>
    /// Advances the tween by a given delta time.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    public void Tick(float delta)
    {
        if (!IsPlaying)
        {
            return;
        }

        float deltaTimeScaled = delta * TimeScale;

        if (_delayRemaining > 0f)
        {
            _delayRemaining -= deltaTimeScaled;
            return;
        }

        Behaviour.Tick(deltaTimeScaled);

        OnTickAction?.Invoke();

        bool isFinished = Behaviour.GetFinished();

        if (!isFinished)
        {
            return;
        }

        MarkFinished();
    }

    /// <summary>
    /// Instantly reaches the final state of the tween, and stops playing.
    /// </summary>
    public void Complete()
    {
        Behaviour.Complete();

        MarkFinished();
    }

    /// <summary>
    /// Kills the tween. This means that the tween will stop playing, leaving it at its current state.
    /// </summary>
    public void Kill()
    {
        if (!IsPlaying)
        {
            return;
        }

        IsPlaying = false;
        IsCompleted = false;
        IsKilled = true;

        Behaviour.Kill();

        OnKillAction?.Invoke();
        OnCompleteOrKillAction?.Invoke();
    }

    /// <summary>
    /// Simulates the progress of a GTween animation for a specified duration.
    /// </summary>
    /// <param name="time">The simulated time in seconds.</param>
    /// <returns>The current GTween instance for method chaining.</returns>
    public TweenInstance Simulate(float time)
    {
        if (!IsPlaying)
        {
            Start();
        }

        float simTime = Math.Min(time, Behaviour.GetDuration());

        float progress = Behaviour.GetElapsed();

        while (simTime > 0f)
        {
            Tick(simTime);

            float newProgress = Behaviour.GetElapsed();
            float tickElapsed = newProgress - progress;
            progress = newProgress;

            simTime -= tickElapsed;
        }

        return this;
    }

    /// <summary>
    /// Adds some delay (in seconds) at the begining of the tween.
    /// </summary>
    public TweenInstance SetDelay(float delaySeconds)
    {
        Delay = delaySeconds;
        return this;
    }

    /// <summary>
    /// Sets the time scale for the tween, affecting its speed.
    /// By default time scale is 1.0f. If you decrease this number, the tween
    /// will update slower. If you increase it, the tween will update faster.
    /// </summary>
    /// <param name="timeScale">The time scale to set.</param>
    /// <returns>The current GTween instance for method chaining.</returns>
    public TweenInstance SetTimeScale(float timeScale)
    {
        TimeScale = timeScale;
        OnTimeScaleChangedAction?.Invoke(timeScale);
        return this;
    }

    /// <summary>
    /// Calculates the total duration of the tween.
    /// </summary>
    public float GetDuration()
    {
        return Behaviour.GetDuration();
    }

    /// <summary>
    /// Calculates the time elapsed since the tween started playing.
    /// </summary>
    public float GetElapsed()
    {
        if(!IsPlaying && !IsCompleted)
        {
            return 0f;
        }

        if(!IsPlaying && IsCompleted)
        {
            return GetDuration();
        }

        return Behaviour.GetElapsed();
    }

    /// <summary>
    /// Gets the time left remaining on the tween (duration - elapsed).
    /// </summary>
    public float GetRemaining()
    {
        if(!IsPlaying && !IsCompleted)
        {
            return GetDuration();
        }

        if(!IsPlaying && IsCompleted)
        {
            return 0f;
        }

        return Behaviour.GetRemaining();
    }

    public TweenInstance OnStart(Action action)
    {
        OnStartAction += action;
        return this;
    }

    public TweenInstance OnTick(Action action)
    {
        OnTickAction += action;
        return this;
    }

    public TweenInstance OnLoop(Action action)
    {
        OnLoopAction += action;
        return this;
    }

    public TweenInstance OnReset(Action action)
    {
        OnResetAction += action;
        return this;
    }

    public TweenInstance OnComplete(Action action)
    {
        OnCompleteAction += action;
        return this;
    }

    public TweenInstance OnKill(Action action)
    {
        OnKillAction += action;
        return this;
    }

    public TweenInstance OnCompleteOrKill(Action action)
    {
        OnCompleteOrKillAction += action;
        return this;
    }

    public TweenInstance OnTimeScaleChanged(Action<float> action)
    {
        OnTimeScaleChangedAction += action;
        return this;
    }

    /// <summary>
    /// Sets the easing function for the tween.
    /// </summary>
    /// <param name="easingFunction">The custom easing function to use.</param>
    /// <returns>The current GTween instance for method chaining.</returns>
    public TweenInstance SetEasing(EasingDelegate easingFunction)
    {
        Behaviour.SetEasing(easingFunction);
        return this;
    }

    /// <summary>
    /// Sets the predefined easing function for the tween.
    /// </summary>
    /// <param name="easing">The predefined easing to use.</param>
    /// <returns>The current GTween instance for method chaining.</returns>
    public TweenInstance SetEasing(Easing easing)
    {
        return SetEasing(PresetEasingDelegateFactory.GetEaseDelegate(easing));
    }


    private void MarkFinished()
    {
        if (!IsPlaying)
        {
            return;
        }

        IsPlaying = false;
        IsCompleted = true;
        IsKilled = false;

        Behaviour.Complete();

        OnCompleteAction?.Invoke();
        OnCompleteOrKillAction?.Invoke();
    }


}
