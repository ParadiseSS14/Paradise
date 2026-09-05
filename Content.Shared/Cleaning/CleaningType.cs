namespace Content.Shared.Cleaning;

[Flags]
public enum CleaningType
{
    Uncleanable = 0,

    Blood = 1 << 0,
    Fingerprints = 1 << 1,
    Fibers = 1 << 2,
    Radiation = 1 << 3,
    Disease = 1 << 4,
    Acid = 1 << 5,
    LightDecal = 1 << 6,
    HardDecal = 1 << 7,
    Scrapeable = 1 << 8,
    Plant = 1 << 9,

    Wash = Blood | Disease | Acid | LightDecal,
    Scrub = Wash | Fingerprints | Fibers | HardDecal,
    All = ~0,
}
