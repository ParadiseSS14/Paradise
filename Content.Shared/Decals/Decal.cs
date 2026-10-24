using System.Numerics;
using Content.Shared.Cleaning;
using Robust.Shared.Serialization;

namespace Content.Shared.Decals
{
    [Serializable, NetSerializable]
    [DataDefinition]
    public sealed partial class Decal
    {
        // if these are made not-readonly, then decal grid state handling needs to be updated to clone decals.
        [DataField("coordinates")] public Vector2 Coordinates = Vector2.Zero;
        [DataField("id")] public  string Id = string.Empty;
        [DataField("color")] public  Color? Color;
        [DataField("angle")] public  Angle Angle = Angle.Zero;
        [DataField("zIndex")] public  int ZIndex;
        // Paradise Change - Cleaning
        [DataField("cleanType")] public  CleaningType CleanType = CleaningType.LightDecal;

        public Decal() {}

        // Paradise Change - Cleaning
        public Decal(Vector2 coordinates, string id, Color? color, Angle angle, int zIndex, CleaningType cleanType)
        {
            Coordinates = coordinates;
            Id = id;
            Color = color;
            Angle = angle;
            ZIndex = zIndex;
            CleanType = cleanType; // Paradise Change - Cleaning
        }

        // Paradise Change START - Cleaning
        public Decal WithCoordinates(Vector2 coordinates) => new(coordinates, Id, Color, Angle, ZIndex, CleanType);
        public Decal WithId(string id) => new(Coordinates, id, Color, Angle, ZIndex, CleanType);
        public Decal WithColor(Color? color) => new(Coordinates, Id, color, Angle, ZIndex, CleanType);
        public Decal WithRotation(Angle angle) => new(Coordinates, Id, Color, angle, ZIndex, CleanType);
        public Decal WithZIndex(int zIndex) => new(Coordinates, Id, Color, Angle, zIndex, CleanType);
        public Decal WithCleanable(CleaningType cleaningType) => new(Coordinates, Id, Color, Angle, ZIndex, cleaningType);
        // Paradise Change END - Cleaning
    }
}
