using Content.Shared.Inventory;

namespace Content.Server.Atmos.Events
{
    public sealed class RefreshPressureProtectiondModifiersEvent : EntityEventArgs
    {
        public List<SlotFlags> TargetSlots { get; } = new() { SlotFlags.HEAD, SlotFlags.OUTERCLOTHING };

        public float LowPressureModifier { get; private set; } = 0f;
        public float LowPressureMultiplier { get; private set; } = 1.0f;
        public float HighPressureModifier { get; private set; } = 0f;
        public float HighPressureMultiplier { get; private set; } = 1.0f;

        public void ModifyProtection(float lowPressureMod, float lowPressureMult, float highPressureMod, float highPressureMult)
        {
            LowPressureModifier += lowPressureMod;
            LowPressureMultiplier *= lowPressureMult;
            HighPressureModifier += highPressureMod;
            HighPressureMultiplier *= highPressureMult;
        }
    }
}
