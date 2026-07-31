using Robust.Shared.ContentPack;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;
using static Robust.Client.Credits.CreditsManager;

namespace Content.Client.Credits;

/// <summary>
///     Contains credits information about the client.
///     Is pretty much a clone of the engine one.
/// </summary>
public static class ClientCreditsManager
{
    /// <summary>
    ///     Gets a list of open source software used in the client and their license.
    /// </summary>
    public static IEnumerable<LicenseEntry> GetLicenses(IResourceManager resources)
    {
        using var file = resources.ContentFileRead("/Credits/Libraries.yml");
        using var reader = new System.IO.StreamReader(file);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);

        foreach (var entry in (YamlSequenceNode)yamlStream.Documents[0].RootNode)
        {
            var mapNode = (YamlMappingNode)entry;
            var name = mapNode.GetNode("name").AsString();
            var license = mapNode.GetNode("license").AsString();

            yield return new LicenseEntry(name, license);
        }
    }
}
