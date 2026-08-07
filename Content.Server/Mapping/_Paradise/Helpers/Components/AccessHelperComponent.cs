using Robust.Shared.Prototypes;
using Content.Shared.Access;

namespace Content.Server._Paradise.AccessHelper
{
    /// <summary>
    /// Used by mapping access helpers
    /// </summary>
    [RegisterComponent, EntityCategory("Mapping")]
    public sealed partial class AccessHelperComponent : Component
    {
        [DataField]
        public ProtoId<AccessLevelPrototype>? Access;
    }
}
