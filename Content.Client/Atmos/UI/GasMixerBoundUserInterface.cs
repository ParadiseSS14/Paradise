using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Trinary.Components;
using Content.Shared.Localizations;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Atmos.UI
{
    /// <summary>
    /// Initializes a <see cref="GasMixerWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class GasMixerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private const float MaxPressure = Atmospherics.MaxOutputPressure;

        [ViewVariables]
        private GasMixerWindow? _window;

        public GasMixerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<GasMixerWindow>();

            _window.ToggleStatusButtonPressed += OnToggleStatusButtonPressed;
            _window.MixerNodeDataSet += RecieveMixerState;

            Update();
        }

        private void RecieveMixerState(GasMixerState obj)
        {
            OnMixerOutputPressurePressed(obj.OutputPressure);
            OnMixerDataSet(obj.MixerOneNodePercentage);
        }

        private void OnToggleStatusButtonPressed()
        {
            if (_window is null) return;
            SendMessage(new GasMixerToggleStatusMessage(_window.MixerState.Enabled));
        }

        private void OnMixerOutputPressurePressed(float outputPressure)
        {
            var pressure = outputPressure;
            if (pressure > MaxPressure)
                pressure = MaxPressure;

            SendMessage(new GasMixerChangeOutputPressureMessage(pressure));
        }

        private void OnMixerDataSet(float nodeA)
        {
            nodeA = Math.Clamp(nodeA, 0f, 100.0f);

            SendMessage(new GasMixerChangeNodePercentageMessage(nodeA));
        }

        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        public override void Update()
        {
            base.Update();
            if (_window == null || !EntMan.TryGetComponent<GasMixerComponent>(Owner, out var mixer))
                return;

            // _window.Title = (mixer.MixerLabel);
            _window.SetMixerStatus(mixer.Enabled);
            _window.SetOutputPressure(mixer.TargetPressure);
            _window.SetNodePercentages(mixer.InletOneConcentration);
        }
    }

    public struct GasMixerState
    {
        public bool Enabled;
        public float OutputPressure;
        /// <summary>
        /// No need for 2, just subtract.
        /// </summary>
        public float MixerOneNodePercentage;
    }
}
