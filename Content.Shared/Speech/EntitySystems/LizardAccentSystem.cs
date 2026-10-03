using Content.Shared.Speech.Components;
using Robust.Shared.Random;
using System.Text.RegularExpressions;

namespace Content.Shared.Speech.EntitySystems;

public sealed partial class LizardAccentSystem : RelayAccentSystem<LizardAccentComponent>
{
    [Dependency] private IRobustRandom _robustRandom = default!;

    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    public override string Accentuate(string message, Entity<LizardAccentComponent>? ent = null)
    {

        // hissss
        message = RegexLowerS.Replace(message, _ => new string('s', _robustRandom.Next(2, 5)));
        // hiSSS
        message = RegexUpperS.Replace(message, _ => new string('S', _robustRandom.Next(2, 5)));

        // ekssit
        message = RegexInternalX.Replace(message, m => m.Groups[1].Value + 'k' + new string('s', _robustRandom.Next(1, 4)));

        // x -> ksss
        message = RegexLowerEndX.Replace(message, m => 'k' + new string('s', _robustRandom.Next(1, 4)) + m.Groups[1].Value);

        // X -> KSSS
        message = RegexUpperEndX.Replace(message, m => 'K' + new string('S', _robustRandom.Next(1, 4)) + m.Groups[1].Value);

        return message;
    }
}
