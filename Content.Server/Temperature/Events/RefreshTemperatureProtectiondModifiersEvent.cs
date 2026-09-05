namespace Content.Server.Temperature.Events
{
    public sealed class RefreshTempratureProtectiondModifiersEvent : EntityEventArgs
    {
        public float CoefficientModofier { get; private set; } = 0.0f;

        public void ModifyProtection(float modifier)
        {
            CoefficientModofier += modifier;
        }
    }
}
