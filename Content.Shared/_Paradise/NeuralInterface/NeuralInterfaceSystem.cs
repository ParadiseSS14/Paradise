using Content.Shared.Alert;
using Content.Shared.Mindshield.Components;
using Content.Shared.Paradise.MindShield;

namespace Content.Shared.Paradise.NeuralInterface;

public sealed partial class NeuralInterfaceSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<NeuralInterfaceComponent, ComponentStartup>(OnCompStartup);
        SubscribeLocalEvent<NeuralInterfaceComponent, MindshieldProtectionGrantedEvent>(OnProtectionGranted);
        SubscribeLocalEvent<NeuralInterfaceComponent, MindshieldProtectionRemovedEvent>(OnProtectionRemoved);

        base.Initialize();
    }

    private void OnCompStartup(Entity<NeuralInterfaceComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.InterfaceType == 0 && HasComp<MindShieldComponent>(ent.Owner))
            ++ent.Comp.InterfaceType;

        _alerts.ShowAlert(ent.Owner, ent.Comp.InterfaceAlertProto, (short)ent.Comp.InterfaceType);
    }

    private void OnProtectionGranted(Entity<NeuralInterfaceComponent> ent, ref MindshieldProtectionGrantedEvent args)
    {
        if (ent.Comp.InterfaceType == 0 && HasComp<MindShieldComponent>(ent.Owner))
            ++ent.Comp.InterfaceType;

        _alerts.ShowAlert(ent.Owner, ent.Comp.InterfaceAlertProto, (short)ent.Comp.InterfaceType);
    }

    private void OnProtectionRemoved(Entity<NeuralInterfaceComponent> ent, ref MindshieldProtectionRemovedEvent args)
    {
        if (ent.Comp.InterfaceType == 1)
            --ent.Comp.InterfaceType;

        _alerts.ShowAlert(ent.Owner, ent.Comp.InterfaceAlertProto, (short)ent.Comp.InterfaceType);
    }
}
