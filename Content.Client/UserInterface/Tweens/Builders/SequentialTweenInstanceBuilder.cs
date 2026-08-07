using Content.Client.UserInterface.Tweens.Behaviours;

namespace Content.Client.UserInterface.Tweens.Builders;

public sealed class SequentialTweenInstanceBuilder
{
    readonly SequenceTweenBehaviour _sequenceTweenBehaviour;
    readonly TweenInstance _tweenInstance;

    private bool _creatingGroupTween;
    private GroupTweenBehaviour? _groupTweenBehaviour;

    SequentialTweenInstanceBuilder()
    {
        _sequenceTweenBehaviour = new SequenceTweenBehaviour();
        _tweenInstance = new TweenInstance(_sequenceTweenBehaviour);
    }

        /// <summary>
    /// Creates a new instance of the <see cref="TweenInstanceSequenceBuilder"/>.
    /// </summary>
    /// <returns>A new instance of the builder.</returns>
    public static SequentialTweenInstanceBuilder New()
    {
        return new SequentialTweenInstanceBuilder();
    }

    /// <summary>
    /// Adds the given tween to the end of the Sequence. This tween will play after all the previous tweens have finished.
    /// </summary>
    /// <param name="TweenInstance">The TweenInstance to append to the sequence.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder Append(TweenInstance TweenInstance)
    {
        _creatingGroupTween = false;

        _sequenceTweenBehaviour.Add(TweenInstance);

        return this;
    }

    /// <summary>
    /// Inserts the given tween at the same time position of the last tween added to the Sequence.
    /// This tween will play at the same time as the previous tween.
    /// </summary>
    /// <param name="TweenInstance">The TweenInstance to join with the sequence.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder Join(TweenInstance TweenInstance)
    {
        if (_creatingGroupTween)
        {
            _groupTweenBehaviour!.Add(TweenInstance);
            return this;
        }

        _creatingGroupTween = true;

        _groupTweenBehaviour = new GroupTweenBehaviour();

        if (_sequenceTweenBehaviour.Tweens.Count > 0)
        {
            TweenInstance previousTween = _sequenceTweenBehaviour.Tweens[^1];
            _sequenceTweenBehaviour.Remove(previousTween);
            _groupTweenBehaviour.Add(previousTween);
        }

        _groupTweenBehaviour.Add(TweenInstance);

        _sequenceTweenBehaviour.Add(new TweenInstance(_groupTweenBehaviour));

        return this;
    }

    /// <summary>
    /// Appends a callback action to the end of the sequence.
    /// </summary>
    /// <param name="callback">The callback action to append.</param>
    /// <param name="callIfCompletingInstantly">Whether to call the callback if the tween is asked to complete instantly.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder AppendCallback(Action callback, bool callIfCompletingInstantly = true)
    {
        CallbackTweenBehaviour callbackTweenBehaviour = new(callback, callIfCompletingInstantly);
        Append(new TweenInstance(callbackTweenBehaviour));

        return this;
    }

    /// <summary>
    /// Inserts the given callback at the same time position of the last tween added to the Sequence.
    /// This tween will play at the same time as the previous tween.
    /// </summary>
    /// <param name="callback">The callback action to append.</param>
    /// <param name="callIfCompletingInstantly">Whether to call the callback if the tween is asked to complete instantly.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder JoinCallback(Action callback, bool callIfCompletingInstantly = true)
    {
        CallbackTweenBehaviour callbackTweenBehaviour = new(callback, callIfCompletingInstantly);
        Join(new TweenInstance(callbackTweenBehaviour));

        return this;
    }

    /// <summary>
    /// Appends a time delay to the end of the sequence.
    /// </summary>
    /// <param name="timeSeconds">The duration of the time delay in seconds.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder AppendTime(float timeSeconds)
    {
        WaitTimeTweenBehaviour timeTweenBehaviour = new(timeSeconds);
        Append(new TweenInstance(timeTweenBehaviour));

        return this;
    }

    /// <summary>
    /// Inserts the given time delay at the same time position of the last tween added to the Sequence.
    /// This tween will play at the same time as the previous tween.
    /// </summary>
    /// <param name="timeSeconds">The duration of the time delay in seconds.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder JoinTime(float timeSeconds)
    {
        WaitTimeTweenBehaviour timeTweenBehaviour = new(timeSeconds);
        Join(new TweenInstance(timeTweenBehaviour));

        return this;
    }

    /// <summary>
    /// Provides a new TweenInstanceSequenceBuilder for building a sequence, and then adds it to the end of the sequence.
    /// </summary>
    /// <param name="createSequence">An action that defines the nested sequence using a new TweenInstanceSequenceBuilder.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder AppendSequence(Action<SequentialTweenInstanceBuilder> createSequence)
    {
        SequentialTweenInstanceBuilder sequenceBuilder = New();
        createSequence.Invoke(sequenceBuilder);
        Append(sequenceBuilder.Build());

        return this;
    }

    /// <summary>
    /// Provides a new TweenInstanceSequenceBuilder for building a sequence, and then inserts it
    /// at the same time position of the last tween added to the Sequence.
    /// </summary>
    /// <param name="createSequence">An action that defines the nested sequence using a new TweenInstanceSequenceBuilder.</param>
    /// <returns>The current instance of the builder.</returns>
    public SequentialTweenInstanceBuilder JoinSequence(Action<SequentialTweenInstanceBuilder> createSequence)
    {
        SequentialTweenInstanceBuilder sequenceBuilder = New();
        createSequence.Invoke(sequenceBuilder);
        Join(sequenceBuilder.Build());

        return this;
    }

    /// <summary>
    /// Builds and returns the final TweenInstance representing the sequence.
    /// </summary>
    /// <returns>The TweenInstance representing the built sequence.</returns>
    public TweenInstance Build() => _tweenInstance;
}
