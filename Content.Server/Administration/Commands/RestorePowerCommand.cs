using Content.Server.Chat.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.Commands;

/// <summary>
/// Recharges every APC to full and re-closes any breaker that has been tripped or flipped, then announces the restoration to the crew.
/// </summary>
[AdminCommand(AdminFlags.Fun)]
public sealed partial class RestorePowerCommand : LocalizedEntityCommands
{
    /// <summary>
    /// Sound collection played alongside the power restoration announcement.
    /// </summary>
    private static readonly ProtoId<SoundCollectionPrototype> PowerOnSound = new("PowerOn");

    [Dependency] private ApcSystem _apc = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private StationSystem _station = default!;

    public override string Command => "restorepower";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 0)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            shell.WriteLine(Help);
            return;
        }

        var count = 0;
        var skipped = 0;

        var query = EntityManager.AllEntityQueryEnumerator<ApcComponent, PowerNetworkBatteryComponent, BatteryComponent>();
        while (query.MoveNext(out var uid, out var apc, out var netBattery, out var battery))
        {
            // Only touch APCs that sit on a grid belonging to a station.
            if (_station.GetOwningStation(uid) is null)
            {
                skipped++;
                continue;
            }

            _battery.SetCharge((uid, battery), battery.MaxCharge);

            if (!apc.MainBreakerEnabled)
                _apc.ApcToggleBreaker(uid, apc, netBattery);

            count++;
        }

        if (count > 0)
        {
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString("restore-power-announcement"),
                announcementSound: new SoundCollectionSpecifier(PowerOnSound, AudioParams.Default.AddVolume(-4f)));
        }

        shell.WriteLine(Loc.GetString("cmd-restorepower-success", ("count", count), ("skipped", skipped)));
    }
}
