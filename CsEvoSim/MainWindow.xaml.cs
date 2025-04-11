using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CsEvoSim.Systems;
using CsEvoSim.Core;
using CsEvoSim.Utils;

namespace CsEvoSim
{
    public partial class MainWindow : Window
    {
        private World _world;
        private DispatcherTimer _timer;
        private SpawnerSystem _spawnerSystem;

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

            // Add systems
            _world.AddSystem(new MovementSystem());
            _world.AddSystem(new RenderSystem(SimulationCanvas));

            _spawnerSystem = new SpawnerSystem(canvasWidth, canvasHeight)
            {
                SpawnRate = 1,
                Interval = 1.0
            };
            _world.AddSystem(_spawnerSystem);

            // Start ECS update loop
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _timer.Tick += (_, _) => _world.Update();
            _timer.Start();
        }

        private void SpawnRateSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_spawnerSystem != null)
                _spawnerSystem.SpawnRate = (int)e.NewValue;
        }

        private void SpawnIntervalSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_spawnerSystem != null)
                _spawnerSystem.Interval = e.NewValue;
        }
    }
}
