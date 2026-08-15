using Content.Client.UserInterface.Tweens.Easers;

namespace Content.Client.UserInterface.Tweens.Tweeners;

public interface ITweener
{
        float Duration { get; }
        float Elapsed { get; }
        float Remaining { get; }

        bool IsPlaying { get; }
        bool IsCompleted { get; }
        bool IsKilled { get; }
        bool IsCompletedOrKilled { get; }

        void SetEasing(EasingDelegate easingFunction);

        void Reset();
        void Start();
        void Tick(float deltaTime);
        void Complete();
        void Kill();
}

