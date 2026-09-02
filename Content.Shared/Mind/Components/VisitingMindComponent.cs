namespace Content.Shared.Mind.Components
{
    [RegisterComponent]
    public sealed partial class VisitingMindComponent : Component
    {
        [ViewVariables]
        public EntityUid? MindId;

        // PARADISE EDIT START - Visiting mind overhaul
        [ViewVariables]
        public bool RedirectChatMessages = true;
        // PARADISE EDIT END
    }

    [ByRefEvent]
    public readonly record struct EntityVisitedEvent(EntityUid MindEntity, MindComponent MindComp)
    {
        public readonly EntityUid MindEntity = MindEntity;

        public readonly MindComponent MindComp = MindComp;
    }

    [ByRefEvent]
    public readonly record struct EntityUnvisitedEvent(EntityUid MindEntity, MindComponent MindComp)
    {
        public readonly EntityUid MindEntity = MindEntity;

        public readonly MindComponent MindComp = MindComp;
    }

    [ByRefEvent]
    public readonly record struct EntityGotUnvisitedEvent(EntityUid MindEntity, MindComponent MindComp)
    {
        public readonly EntityUid MindEntity = MindEntity;

        public readonly MindComponent MindComp = MindComp;
    }
}
