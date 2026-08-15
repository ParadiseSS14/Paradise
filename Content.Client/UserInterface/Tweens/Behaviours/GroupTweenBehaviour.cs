using Content.Client.UserInterface.Tweens.Easers;
using Content.Client.UserInterface.Tweens.Extensions;

namespace Content.Client.UserInterface.Tweens.Behaviours;

    public sealed class GroupTweenBehaviour : TweenBehaviour
    {
        public IReadOnlyList<TweenInstance> Tweens => _tweens;

        readonly List<TweenInstance> _tweens = new();
        readonly List<TweenInstance> _playintweenInstances = new();

        bool _durationCalculated;
        float _cachedCalculatedDuration;

        public override void Start(bool isCompletingInstantly)
        {
            StartTweens(isCompletingInstantly);
        }

        public override void Tick(float deltaTime)
        {
            for (int i = _playintweenInstances.Count - 1; i >= 0; --i)
            {
                TweenInstance tweenInstance = _playintweenInstances[i];

                tweenInstance.Tick(deltaTime);

                if (!tweenInstance.IsPlaying)
                {
                    _playintweenInstances.RemoveAt(i);
                }
            }

            if (_playintweenInstances.Count == 0)
            {
                MarkFinished();
            }
        }

        public override void Kill()
        {
            foreach (TweenInstance tween in _playintweenInstances)
            {
                tween.Kill();
            }

            _playintweenInstances.Clear();

            MarkFinished();
        }

        public override void Complete()
        {
            foreach (TweenInstance tween in _playintweenInstances)
            {
                if (tween.IsCompleted)
                {
                    continue;
                }

                if (!tween.IsPlaying)
                {
                    tween.Start(isCompletingInstantly: true);
                }

                tween.Complete();
            }

            _playintweenInstances.Clear();

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

        public void Add(TweenInstance tweenInstance)
        {
            if(tweenInstance.IsPlayingOrCompletedOrNested())
            {
                return;
            }

            tweenInstance.IsNested = true;

            _tweens.Add(tweenInstance);

            _durationCalculated = false;
        }

        void StartTweens(bool isCompletingInstantly)
        {
            _playintweenInstances.Clear();
            _playintweenInstances.AddRange(_tweens);

            for (int i = _playintweenInstances.Count - 1; i >= 0; --i)
            {
                TweenInstance tweenInstance = _playintweenInstances[i];

                tweenInstance.Start(isCompletingInstantly);

                if (!tweenInstance.IsPlaying)
                {
                    _playintweenInstances.RemoveAt(i);
                }
            }

            if (_playintweenInstances.Count == 0)
            {
                MarkFinished();
            }
        }
    }
