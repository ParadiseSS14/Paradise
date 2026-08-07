namespace Content.Client.UserInterface.Tweens;

public sealed class TweenManager : EntitySystem
{
    public float TimeScale { get; set; } = 1f;

    private readonly List<TweenInstance> _aliveTweens = new();
    private readonly List<TweenInstance> _tweensToAdd = new();
    private readonly List<TweenInstance> _tweensToRemove = new();


    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        Tick(frameTime);
    }

    public void Play(TweenInstance tween)
    {
        if (tween.IsNested)
        {
            return;
        }

        if (tween.IsAlive)
        {
            TryStartTween(tween);
            return;
        }

        tween.IsAlive = true;
        _aliveTweens.Add(tween);

        TryStartTween(tween);
    }

    public void Tick(float deltaTime)
    {
        float scaledDeltaTime = deltaTime * TimeScale;

        foreach (var tween in _aliveTweens)
        {
            if (tween.IsPlaying)
            {
                tween.Tick(scaledDeltaTime);
            }
            else
            {
                _tweensToRemove.Add(tween);
            }
        }

        foreach (var tween in _tweensToRemove)
        {
            tween.IsAlive = false;

            _aliveTweens.Remove(tween);
            _tweensToAdd.Remove(tween);
        }

        _tweensToRemove.Clear();
    }

    public void Clear()
    {
        _aliveTweens.Clear();
        _tweensToAdd.Clear();
        _tweensToRemove.Clear();
    }

    private void TryStartTween(TweenInstance tween)
    {
        if (!tween.IsPlaying)
        {
            tween.Start();
        }
    }
}
