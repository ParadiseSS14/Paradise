using Content.Client.UserInterface.Tweens.Easers;

namespace Content.Client.UserInterface.Tweens;

public interface ITweenBehaviour
{
    float GetDuration();
    float GetElapsed();
    float GetRemaining();
    bool GetFinished();
    bool IsLoopable();
    void SetEasing(EasingDelegate easingFunction);

    void Start(bool isCompletingInstantly);
    void Tick(float delta);
    void Kill();
    void Complete();

    // void SetEase(Ease)
}

