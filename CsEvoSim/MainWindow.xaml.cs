using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Dictionary<string, ISystemWithSettings> _systemsWithSettings = new();
        private bool _paused = false;

        // FPS counter variables
        private Stopwatch _fpsStopwatch;
        private int _frameCount;
        private double _elapsedTime;
        private const double FPS_UPDATE_INTERVAL = 0.5; // Update FPS display every 0.5 seconds

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => InitializeSimulation();
            SizeChanged += MainWindow_SizeChanged;
        }

        private void InitializeSimulation()
        {
            double canvasWidth = SimulationCanvas.ActualWidth;
            double canvasHeight = SimulationCanvas.ActualHeight;

            _world = new World();
            _systemsWithSettings.Clear();

            // Create systems
            var energySystem = new EnergySystem();
            var feedingSystem = new FeedingSystem(); // Add this
            var movementSystem = new MovementSystem();
            movementSystem.SetCanvasDimensions(canvasWidth, canvasHeight);
            var renderSystem = new RenderSystem(SimulationCanvas);
            var spawnerSystem = new SpawnerSystem(canvasWidth, canvasHeight);

            // Add systems to world in appropriate order
            _world.AddSystem(energySystem);
            _world.AddSystem(feedingSystem); // Add this
            _world.AddSystem(movementSystem);
            _world.AddSystem(renderSystem);
            _world.AddSystem(spawnerSystem);

            // Track systems with settings
            _systemsWithSettings["Energy"] = energySystem;
            _systemsWithSettings["Feeding"] = feedingSystem; // Add this
            _systemsWithSettings["Movement"] = movementSystem;
            _systemsWithSettings["Rendering"] = renderSystem;
            _systemsWithSettings["Spawner"] = spawnerSystem;

            // Generate dynamic UI for system settings
            BuildSettingsMenu();

            // Initialize FPS counter
            _fpsStopwatch = new Stopwatch();
            _fpsStopwatch.Start();
            _frameCount = 0;
            _elapsedTime = 0;

            // Set initial play button state
            PausePlayButton.IsChecked = _paused;

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
                    UpdateFpsCounter();
                }
            };

            _timer.Start();
        }

        private void UpdateFpsCounter()
        {
            _frameCount++;

            // Calculate time since last update
            double elapsed = _fpsStopwatch.Elapsed.TotalSeconds;
            _elapsedTime += elapsed;
            _fpsStopwatch.Restart();

            // Update display approximately every half second
            if (_elapsedTime >= FPS_UPDATE_INTERVAL)
            {
                double fps = _frameCount / _elapsedTime;
                FpsCounterLabel.Text = $"FPS: {fps:0.0}";

                // Reset counters
                _frameCount = 0;
                _elapsedTime = 0;
            }
        }

        private void BuildSettingsMenu()
        {
            // Clear existing items
            SettingsMenuItem.Items.Clear();

            // Add general settings
            var generalMenuItem = new MenuItem { Header = "_General" };
            var pauseMenuItem = new MenuItem
            {
                Header = "Pause/Resume",
                IsCheckable = true,
                IsChecked = _paused
            };
            pauseMenuItem.Click += PauseResume_Click;
            generalMenuItem.Items.Add(pauseMenuItem);
            SettingsMenuItem.Items.Add(generalMenuItem);

            // Add separator between general and system-specific settings
            SettingsMenuItem.Items.Add(new Separator());

            // Add each system's settings
            foreach (var system in _systemsWithSettings.Values)
            {
                var systemMenuItem = new MenuItem { Header = $"_{system.SettingsGroupName}" };

                foreach (var setting in system.GetSettings())
                {
                    var settingElement = SettingsUIFactory.CreateUIElement(setting);
                    var menuItem = new MenuItem { Header = settingElement };
                    systemMenuItem.Items.Add(menuItem);
                }

                SettingsMenuItem.Items.Add(systemMenuItem);
            }
        }

        private void ResetSimulation_Click(object sender, RoutedEventArgs e)
        {
            InitializeSimulation();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PauseResume_Click(object sender, RoutedEventArgs e)
        {
            _paused = !_paused;

            // Update pause/play button state to match
            PausePlayButton.IsChecked = _paused;

            // Update menu item if it's the source of the click
            if (sender is MenuItem menuItem)
                menuItem.IsChecked = _paused;
        }

        private void PausePlayButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _paused = PausePlayButton.IsChecked ?? false;

            // Update menu item to match button state
            var menuItem = SettingsMenuItem.Items[0] as MenuItem;
            if (menuItem != null && menuItem.Items.Count > 0 && menuItem.Items[0] is MenuItem pauseMenuItem)
            {
                pauseMenuItem.IsChecked = _paused;
            }
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            bool showStats = menuItem?.IsChecked ?? false;
            StatisticsPanel.Visibility = showStats ? Visibility.Visible : Visibility.Collapsed;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CsEvoSim - Evolution Simulation\nVersion 1.0",
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update movement system with new canvas dimensions
            if (_systemsWithSettings.TryGetValue("Movement", out var system) &&
                system is MovementSystem movementSystem)
            {
                movementSystem.SetCanvasDimensions(SimulationCanvas.ActualWidth, SimulationCanvas.ActualHeight);
            }
        }
    }
}
