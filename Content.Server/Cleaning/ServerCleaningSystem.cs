using Content.Server.Forensics;
using Content.Shared.Cleaning;

namespace Content.Server.Cleaning;

public sealed class ServerCleaningSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CleaningComponent, CleaningFinishedEvent>(OnCleaning);
    }

    private void OnCleaning(Entity<CleaningComponent> ent, ref CleaningFinishedEvent args)
    {

        if (args.Target is not { } targetNet)
            return;

        DoForensicsClean(targetNet, args);
    }

    private void DoForensicsClean(EntityUid ent, CleaningFinishedEvent args)
    {
        if (!TryComp<ForensicsComponent>(ent, out var forensics))
        {
            return;
        }
        forensics.Fingerprints.Clear();
        forensics.Fibers.Clear();

        if (forensics.CanDnaBeCleaned)
            forensics.DNAs.Clear();

        if (args.Used is { } cleanerEntity &&
            TryComp<ResidueComponent>(cleanerEntity, out var residue))
        {
            forensics.Residues.Add(
                string.IsNullOrEmpty(residue.ResidueColor)
                    ? Loc.GetString(
                        "forensic-residue",
                        ("adjective", residue.ResidueAdjective))
                    : Loc.GetString(
                        "forensic-residue-colored",
                        ("color", residue.ResidueColor),
                        ("adjective", residue.ResidueAdjective)));
        }
    }
}
