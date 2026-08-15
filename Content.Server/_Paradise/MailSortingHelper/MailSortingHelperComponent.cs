using Robust.Shared.Prototypes;

namespace Content.Server._Paradise.MailSortingHelper
{
    /// <summary>
    /// Used by mapping mail sorting helper. What ID to apply to the below disposal router.
    /// </summary>
    [RegisterComponent, EntityCategory("Mapping")]
    public sealed partial class MailSortingHelperComponent : Component
    {
        [DataField]
        public HashSet<string> Mailtag = new();
    }
}
