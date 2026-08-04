using Content.Server.DeviceLinking.Components;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
//Paradise Content - Para13 Buttons
using Content.Server.Power.Components;
using Content.Shared.Power;
using Robust.Server.GameObjects;

namespace Content.Server.DeviceLinking.Systems;

public sealed partial class SignalSwitchSystem : EntitySystem
{
    [Dependency] private DeviceLinkSystem _deviceLink = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private LockSystem _lock = default!;
    //Paradise Content - Para13 Buttons
    [Dependency] private AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignalSwitchComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SignalSwitchComponent, ActivateInWorldEvent>(OnActivated);
        //Paradise Content - Para13 Buttons
        SubscribeLocalEvent<SignalSwitchComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnInit(EntityUid uid, SignalSwitchComponent comp, ComponentInit args)
    {
        _deviceLink.EnsureSourcePorts(uid, comp.OnPort, comp.OffPort, comp.StatusPort);

        //Paradise Content - Para13 Buttons
        if (TryComp<ApcPowerReceiverComponent>(uid, out var receiver))
            _appearance.SetData(uid, PowerDeviceVisuals.Powered, receiver.Powered);
    }

    private void OnActivated(EntityUid uid, SignalSwitchComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (_lock.IsLocked(uid))
            return;

        //Paradise Content - Para13 Buttons
        if (TryComp<ApcPowerReceiverComponent>(uid, out var receiver) && !receiver.Powered)
            return;

        comp.State = !comp.State;
        _deviceLink.InvokePort(uid, comp.State ? comp.OnPort : comp.OffPort);

        // only send status if it's a toggle switch and not a button
        if (comp.OnPort != comp.OffPort)
        {
            _deviceLink.SendSignal(uid, comp.StatusPort, comp.State);
        }

        var audioParams = comp.ClickSound?.Params ?? AudioParams.Default;
        audioParams = audioParams.WithVariation(0.125f).AddVolume(8f);
        _audio.PlayPvs(comp.ClickSound, uid, audioParams);

        args.Handled = true;
    }

//Paradise Content - Para13 Buttons
    private void OnPowerChanged(EntityUid uid, SignalSwitchComponent component, PowerChangedEvent args)
    {
        _appearance.SetData(uid, PowerDeviceVisuals.Powered, args.Powered);
    }
}
