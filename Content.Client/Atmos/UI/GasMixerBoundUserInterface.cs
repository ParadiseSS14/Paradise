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
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not GasMixerBoundUserInterfaceState cast)
                return;

            _window.Title = (cast.MixerLabel);
            _window.SetMixerStatus(cast.Enabled);
            _window.SetOutputPressure(cast.OutputPressure);
            _window.SetNodePercentages(cast.NodeOne);
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
