using Content.Client.UserInterface.Tweens.Easers;
using Content.Client.UserInterface.Tweens.Tweeners;

namespace Content.Client.UserInterface.Tweens.Behaviours;

public sealed class InterpolationTweenBehaviour : TweenBehaviour
{
    readonly List<ITweener> _tweeners = new();
    readonly List<ITweener> _playingTweeners = new();

    private bool _durationCalculated;
    private float _cachedDurationCalculated;

    public override void Start(bool startInstantly)
    {
        StartTweeners();
    }

    public override void Tick(float deltaTime)
    {
        for (int i = _playingTweeners.Count - 1; i >= 0; i--)
        {
            ITweener tweener = _playingTweeners[i];
            tweener.Tick(deltaTime);

            if (!tweener.IsPlaying)
            {
                _playingTweeners.RemoveAt(i);
            }
        }

        if (_playingTweeners.Count == 0)
        {
            MarkFinished();
        }
    }

    public override void Kill()
    {
        foreach (var tweener in _playingTweeners)
        {
            tweener.Kill();
        }

        _playingTweeners.Clear();

        MarkFinished();
    }

    public override void Complete()
    {
        foreach (var tweener in _playingTweeners)
        {
            if (!tweener.IsPlaying)
            {
                continue;
            }

            tweener.Complete();
        }

        _playingTweeners.Clear();

        MarkFinished();
    }

    public override void SetEasing(EasingDelegate easingFunction)
    {
        foreach (ITweener tweener in _tweeners)
        {
            tweener.SetEasing(easingFunction);
        }
    }

    public override float GetDuration()
    {
        if (_durationCalculated)
        {
            return _cachedDurationCalculated;
        }

        _durationCalculated = true;

        _cachedDurationCalculated = 0.0f;

        foreach (ITweener tweener in _tweeners)
        {
            _cachedDurationCalculated += tweener.Duration;
        }

        return _cachedDurationCalculated;
    }

    public override float GetElapsed()
    {
        float totalElapsed = 0.0f;

        foreach (ITweener tweener in _tweeners)
        {
            totalElapsed += tweener.Elapsed;
        }

        return totalElapsed;
    }

    public void Add(ITweener tweener)
    {
        if (tweener == null)
        {
            return;
        }

        if (tweener.IsPlaying)
        {
            return;
        }

        if (_tweeners.Contains(tweener))
        {
            return;
        }

        _tweeners.Add(tweener);

        _durationCalculated = false;
    }

    void StartTweeners()
    {
        _playingTweeners.Clear();
        _playingTweeners.AddRange(_tweeners);

        for (int i = _playingTweeners.Count - 1; i >= 0; --i)
        {
            ITweener tweener = _playingTweeners[i];

            tweener.Start();

            if (!tweener.IsPlaying)
            {
                _playingTweeners.RemoveAt(i);
            }
        }

        if (_playingTweeners.Count == 0)
        {
            MarkFinished();
        }
    }
}
