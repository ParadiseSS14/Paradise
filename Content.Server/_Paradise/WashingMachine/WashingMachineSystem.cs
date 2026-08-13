using Content.Server.Power.Components;
using Content.Shared._Paradise.WashingMachine;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Paradise.WashingMachine;

public sealed partial class WashingMachineSystem : SharedWashingMachineSystem
{
    [Dependency] private SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private SharedAudioSystem _audioSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    protected override void StartWash(Entity<WashingMachineComponent> entity)
    {
        if (!(TryComp<ApcPowerReceiverComponent>(entity.Owner, out var apc) && apc.Powered))
            return;

        entity.Comp.PlayingStream = _audioSystem.PlayPvs(entity.Comp.RunningSound, entity.Owner, AudioParams.Default.WithLoop(true).WithMaxDistance(2f))?.Entity;
        entity.Comp.WashEndTime = _timing.CurTime + entity.Comp.WashDuration;
        SetState(entity, WashingMachineVisualState.Running);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WashingMachineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.State != WashingMachineVisualState.Running)
                continue;
            if (TryComp<ApcPowerReceiverComponent>(uid, out var apc) && !apc.Powered)
            {
                SetState((uid, comp), WashingMachineVisualState.Closed);
                _audioSystem.Stop(comp.PlayingStream);
                comp.PlayingStream = null;
                continue;
            }
            if (_timing.CurTime <= comp.WashEndTime)
                continue;
            // Ideally there'd be some code in here to wash the items inside, but we don't HAVE blood staining mechanics!! (yet)
            // Set our running state to false, update our appearance, and play a little ding.
            SetState((uid, comp), WashingMachineVisualState.Closed);
            _audioSystem.Stop(comp.PlayingStream);
            comp.PlayingStream = null;
            _audioSystem.PlayPvs(comp.FinishSound, uid, AudioParams.Default.WithVolume(-5f).WithMaxDistance(2f));
        }
    }

    protected override void SetState(Entity<WashingMachineComponent> entity, WashingMachineVisualState state)
    {
        if (entity.Comp.State == state)
            return;

        entity.Comp.State = state;
        _appearanceSystem.SetData(
            entity.Owner,
            WashingMachineVisual.State,
            state);
    }
}
