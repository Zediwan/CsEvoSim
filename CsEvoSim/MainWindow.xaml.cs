using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CsEvoSim.Core;
using CsEvoSim.Systems;
using CsEvoSim.Utils;

namespace CsEvoSim
{
    public partial class MainWindow : Window
    {
        private World _world;
        private DispatcherTimer _timer;
        private SpawnerSystem _spawnerSystem;
        private bool _paused = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => InitializeSimulation();
        }

        private void InitializeSimulation()
        {
            double canvasWidth = SimulationCanvas.ActualWidth;
            double canvasHeight = SimulationCanvas.ActualHeight;

            _world = new World();

            _spawnerSystem = new SpawnerSystem(canvasWidth, canvasHeight)
            {
                SpawnRate = (int)SpawnRateSlider.Value,
                Interval = SpawnIntervalSlider.Value,
                IsEnabled = EnableSpawningCheckBox.IsChecked ?? true
            };

            _world.AddSystem(new MovementSystem());
            _world.AddSystem(new RenderSystem(SimulationCanvas));
            _world.AddSystem(_spawnerSystem);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            _timer.Tick += (_, _) =>
            {
                if (!_paused)
                {
                    _world.Update();
                    OrganismCountLabel.Text = $"Organisms: {_world.Entities.Count}";
                }
            };

            _timer.Start();
        }

        private void SpawnRateSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_spawnerSystem == null) return;

            int rate = (int)e.NewValue;
            _spawnerSystem.SpawnRate = rate;
            SpawnRateValue.Text = rate.ToString();
        }

        private void SpawnIntervalSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_spawnerSystem == null) return;

            double interval = Math.Round(e.NewValue, 2);
            _spawnerSystem.Interval = interval;
            SpawnIntervalValue.Text = interval.ToString("0.00");
        }

        private void PauseResume_Click(object sender, RoutedEventArgs e)
        {
            _paused = !_paused;
        }

        private void EnableSpawningCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_spawnerSystem == null) return;

            bool isEnabled = EnableSpawningCheckBox.IsChecked ?? false;

            // Update spawner system
            _spawnerSystem.IsEnabled = isEnabled;

            // Update UI controls
            SpawnRateSlider.IsEnabled = isEnabled;
            SpawnIntervalSlider.IsEnabled = isEnabled;

            // Optional: visual greying out
            SpawnRateSlider.Opacity = isEnabled ? 1.0 : 0.5;
            SpawnRateValue.Opacity = isEnabled ? 1.0 : 0.5;
            SpawnIntervalSlider.Opacity = isEnabled ? 1.0 : 0.5;
            SpawnIntervalValue.Opacity = isEnabled ? 1.0 : 0.5;
        }
    }
}
