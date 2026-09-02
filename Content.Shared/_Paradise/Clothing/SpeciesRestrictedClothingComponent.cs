using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Paradise.Clothing;

[RegisterComponent]
public sealed partial class SpeciesRestrictedClothingComponent : Component
{
    [DataField]
    public List<ProtoId<SpeciesPrototype>> AllowedSpecies = new List<ProtoId<SpeciesPrototype>>();

    [DataField]
    public List<ProtoId<SpeciesPrototype>> BannedSpecies = new List<ProtoId<SpeciesPrototype>>();

}
