using Content.Client.UserInterface.Tweens.Easers;
using Content.Client.UserInterface.Tweens.Extensions;

namespace Content.Client.UserInterface.Tweens.Behaviours;

    public sealed class SequenceTweenBehaviour : TweenBehaviour
    {
        public IReadOnlyList<TweenInstance> Tweens => _tweens;

        readonly List<TweenInstance> _tweens = new();
        readonly List<TweenInstance> _playinTweenInstances = new();

        bool _durationCalculated;
        float _cachedCalculatedDuration;

        public override void Start(bool isCompletingInstantly)
        {
            StartTweens(isCompletingInstantly);
        }

        public override void Tick(float deltaTime)
        {
            if (_playinTweenInstances.Count == 0)
            {
                MarkFinished();
                return;
            }

            TweenInstance TweenInstance = _playinTweenInstances[0];

            TweenInstance.Tick(deltaTime);

            if (TweenInstance.IsPlaying)
            {
                return;
            }

            _playinTweenInstances.RemoveAt(0);

            if (_playinTweenInstances.Count > 0)
            {
                TweenInstance nextTweenInstance = _playinTweenInstances[0];

                nextTweenInstance.Start();
            }
            else
            {
                Tick(deltaTime);
            }
        }

        public override void Kill()
        {
            foreach (TweenInstance tween in _playinTweenInstances)
            {
                tween.Kill();
            }

            _playinTweenInstances.Clear();

            MarkFinished();
        }

        public override void Complete()
        {
            foreach (TweenInstance tween in _tweens)
            {
                if(tween.IsCompleted)
                {
                    continue;
                }

                if (!tween.IsPlaying)
                {
                    tween.Start();
                }

                tween.Complete();
            }

            _playinTweenInstances.Clear();

            MarkFinished();
        }

        public override void SetEasing(EasingDelegate easingFunction)
        {
            foreach (TweenInstance tween in _tweens)
            {
                tween.SetEasing(easingFunction);
            }
        }

        public override float GetDuration()
        {
            if(_durationCalculated)
            {
                return _cachedCalculatedDuration;
            }

            _durationCalculated = true;

            _cachedCalculatedDuration = 0.0f;

            foreach (TweenInstance tween in _tweens)
            {
                _cachedCalculatedDuration += tween.GetDuration();
            }

            return _cachedCalculatedDuration;
        }

        public override float GetElapsed()
        {
            float totalDuration = 0.0f;

            foreach (TweenInstance tween in _tweens)
            {
                totalDuration += tween.GetElapsed();
            }

            return totalDuration;
        }

        public void Add(TweenInstance TweenInstance)
        {
            if (TweenInstance.IsPlayingOrCompletedOrNested())
            {
                return;
            }

            TweenInstance.IsNested = true;

            _tweens.Add(TweenInstance);

            _durationCalculated = false;
        }

        public void Remove(TweenInstance TweenInstance)
        {
            if (TweenInstance.IsPlayingOrCompleted())
            {
                return;
            }

            bool found = _tweens.Remove(TweenInstance);

            if (!found)
            {
                return;
            }

            TweenInstance.IsNested = false;

            _durationCalculated = false;
        }

        void StartTweens(bool isCompletingInstantly)
        {
            _playinTweenInstances.Clear();
            _playinTweenInstances.AddRange(_tweens);

            if (_playinTweenInstances.Count > 0)
            {
                TweenInstance TweenInstance = _playinTweenInstances[0];
                TweenInstance.Start(isCompletingInstantly);
            }
            else
            {
                MarkFinished();
            }
        }
    }
