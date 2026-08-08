namespace Content.Server.Atmos.Events
{
    public sealed partial class RefreshPressureProtectionEvent : EntityEventArgs
    {
        public EntityUid Performer;
        public float HighPressureMultiplier = 1f;
        public float HighPressureModifier;
        public float LowPressureMultiplier = 1f;
        public float LowPressureModifier;
        public RefreshPressureProtectionEvent(EntityUid performer, float highPressureMultiplier, float highPressureModifier, float lowPressureMultiplier, float lowPressureModifier)
        {
            Performer = performer;
            HighPressureMultiplier = highPressureMultiplier;
            HighPressureModifier = highPressureModifier;
            LowPressureMultiplier = lowPressureMultiplier;
            LowPressureModifier = lowPressureModifier;
        }
    }
}
