using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Systems
{
    public class RenderSystem : ISystemWithSettings
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<Entity, Ellipse> _entityVisuals = new();
        private readonly Dictionary<Entity, UIElement[]> _entityStatusBars = new();
        private bool _showDebugInfo = false;
        private bool _showFoodType = true;

        // Track entities that exist in the simulation
        private HashSet<Entity> _currentEntities = new();

        // Colors for status bars
        private readonly SolidColorBrush _healthBarFill = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush _energyBarFill = new SolidColorBrush(Colors.Yellow);
        private readonly SolidColorBrush _barBorder = new SolidColorBrush(Colors.White);

        // Icons for organism type
        private readonly SolidColorBrush _plantIcon = new SolidColorBrush(Colors.Green);
        private readonly SolidColorBrush _meatIcon = new SolidColorBrush(Colors.Red);

        // Status bar configuration
        private const double BarHeight = 2;
        private const double BarSpacing = 1;
        private const double BarOffset = 5;
        private const double IconSize = 4;

        public bool ShowDebugInfo
        {
            get => _showDebugInfo;
            set => _showDebugInfo = value;
        }

        public bool ShowFoodType
        {
            get => _showFoodType;
            set => _showFoodType = value;
        }

        public string SettingsGroupName => "Rendering";

        public RenderSystem(Canvas canvas)
        {
            _canvas = canvas;
        }

        public IEnumerable<SystemSetting> GetSettings()
        {
            yield return SystemSetting.CreateBoolean(
                "ShowDebugInfo",
                "Show Debug Information",
                _showDebugInfo,
                val => _showDebugInfo = val,
                "Display additional debug visualizations for each organism"
            );

            yield return SystemSetting.CreateBoolean(
                "ShowFoodType",
                "Show Food Type",
                _showFoodType,
                val => _showFoodType = val,
                "Display indicators for organism food type (plant/meat)"
            );
        }

        public void Update(List<Entity> entities)
        {
            // Clear tracking set for current update cycle
            _currentEntities.Clear();

            // Add all current entities to tracking set
            foreach (var entity in entities)
            {
                _currentEntities.Add(entity);
            }

            // Remove visuals for entities that no longer exist
            var entitiesToRemove = _entityVisuals.Keys
                .Where(entity => !_currentEntities.Contains(entity))
                .ToList();

            foreach (var deadEntity in entitiesToRemove)
            {
                // Remove organism visual
                if (_entityVisuals.TryGetValue(deadEntity, out var visual))
                {
                    _canvas.Children.Remove(visual);
                    _entityVisuals.Remove(deadEntity);
                }

                // Remove status bars if they exist
                if (_entityStatusBars.TryGetValue(deadEntity, out var bars))
                {
                    foreach (var bar in bars)
                    {
                        _canvas.Children.Remove(bar);
                    }
                    _entityStatusBars.Remove(deadEntity);
                }
            }

            // Update visuals for existing entities
            foreach (var entity in entities)
            {
                var pos = entity.GetComponent<PositionComponent>();
                var dna = entity.GetComponent<DNAComponent>();
                var energy = entity.GetComponent<EnergyComponent>();

                if (pos == null || dna == null) continue;

                double entityRadius = dna.Size / 2;
                double entityCenterX = pos.X;
                double entityCenterY = pos.Y;

                // Create organism visual if missing
                if (!_entityVisuals.ContainsKey(entity))
                {
                    var ellipse = new Ellipse
                    {
                        Width = dna.Size,
                        Height = dna.Size,
                        Fill = new SolidColorBrush(dna.Color)
                    };
                    _entityVisuals[entity] = ellipse;
                    _canvas.Children.Add(ellipse);
                }

                var shape = _entityVisuals[entity];

                // Update position (centered)
                Canvas.SetLeft(shape, entityCenterX - entityRadius);
                Canvas.SetTop(shape, entityCenterY - entityRadius);

                // Update color and status bars based on debug mode and energy component
                if (energy != null)
                {
                    if (_showDebugInfo)
                    {
                        // Ensure status bars exist for this entity
                        if (!_entityStatusBars.ContainsKey(entity))
                        {
                            CreateStatusBars(entity, entityRadius, energy);
                        }

                        // Get status bars
                        var statusBars = _entityStatusBars[entity];

                        // First 4 elements are health/energy bars
                        var healthBarBg = (Rectangle)statusBars[0];
                        var healthBarFg = (Rectangle)statusBars[1];
                        var energyBarBg = (Rectangle)statusBars[2];
                        var energyBarFg = (Rectangle)statusBars[3];

                        // Food type indicators are next (if used)
                        Ellipse foodTypeIcon = null;
                        Ellipse digestionIcon = null;

                        if (_showFoodType && statusBars.Length > 5)
                        {
                            foodTypeIcon = (Ellipse)statusBars[4];
                            digestionIcon = (Ellipse)statusBars[5];
                        }

                        // Update status bar positions
                        double barWidth = entityRadius * 2;
                        double healthBarY = entityCenterY - entityRadius - BarOffset - BarHeight * 2 - BarSpacing;
                        double energyBarY = entityCenterY - entityRadius - BarOffset - BarHeight;

                        // Position food type icon if used
                        if (_showFoodType && foodTypeIcon != null && digestionIcon != null)
                        {
                            // Position to the right of the bars
                            Canvas.SetLeft(foodTypeIcon, entityCenterX + entityRadius + 2);
                            Canvas.SetTop(foodTypeIcon, healthBarY);

                            Canvas.SetLeft(digestionIcon, entityCenterX + entityRadius + 2);
                            Canvas.SetTop(digestionIcon, energyBarY);

                            // Update the digestion icon color based on spectrum
                            if (digestionIcon != null)
                            {
                                Color digestionColor;
                                if (energy.DigestionSpectrum <= -0.8)
                                    digestionColor = Colors.Green;  // Pure herbivore
                                else if (energy.DigestionSpectrum >= 0.8)
                                    digestionColor = Colors.Red;    // Pure carnivore
                                else
                                {
                                    // Blend between green and red for omnivores
                                    byte r = (byte)(128 + (energy.DigestionSpectrum * 127));
                                    byte g = (byte)(128 - (energy.DigestionSpectrum * 127));
                                    byte b = 0;
                                    digestionColor = Color.FromRgb(r, g, b);
                                }
                                digestionIcon.Fill = new SolidColorBrush(digestionColor);
                            }

                            // Show/hide based on settings
                            foodTypeIcon.Visibility = _showFoodType ? Visibility.Visible : Visibility.Collapsed;
                            digestionIcon.Visibility = _showFoodType ? Visibility.Visible : Visibility.Collapsed;
                        }

                        // Position health bar background
                        Canvas.SetLeft(healthBarBg, entityCenterX - entityRadius);
                        Canvas.SetTop(healthBarBg, healthBarY);
                        healthBarBg.Width = barWidth;

                        // Position and scale health bar foreground based on health percentage
                        Canvas.SetLeft(healthBarFg, entityCenterX - entityRadius);
                        Canvas.SetTop(healthBarFg, healthBarY);
                        healthBarFg.Width = Math.Max(0, barWidth * energy.HealthPercentage);

                        // Position energy bar background
                        Canvas.SetLeft(energyBarBg, entityCenterX - entityRadius);
                        Canvas.SetTop(energyBarBg, energyBarY);
                        energyBarBg.Width = barWidth;

                        // Position and scale energy bar foreground based on energy percentage
                        Canvas.SetLeft(energyBarFg, entityCenterX - entityRadius);
                        Canvas.SetTop(energyBarFg, energyBarY);
                        energyBarFg.Width = Math.Max(0, barWidth * energy.EnergyPercentage);

                        // Make bars visible
                        foreach (var bar in statusBars)
                        {
                            bar.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        // Hide status bars when debug mode is off
                        if (_entityStatusBars.TryGetValue(entity, out var statusBars))
                        {
                            foreach (var bar in statusBars)
                            {
                                bar.Visibility = Visibility.Collapsed;
                            }
                        }
                    }

                    // Update organism color based on energy status
                    if (energy.Energy <= 0)
                    {
                        // Darken organism when out of energy
                        Color baseColor = dna.Color;
                        Color darkColor = Color.FromRgb(
                            (byte)(baseColor.R * 0.7),
                            (byte)(baseColor.G * 0.7),
                            (byte)(baseColor.B * 0.7)
                        );
                        shape.Fill = new SolidColorBrush(darkColor);
                    }
                    else
                    {
                        // Normal color
                        shape.Fill = new SolidColorBrush(dna.Color);
                    }
                }
            }
        }

        private void CreateStatusBars(Entity entity, double radius, EnergyComponent energy)
        {
            // List to hold all the UI elements
            List<UIElement> statusElements = new List<UIElement>();

            // Create health bar background (black)
            var healthBarBg = new Rectangle
            {
                Height = BarHeight,
                Fill = Brushes.Black,
                Stroke = _barBorder,
                StrokeThickness = 0.5
            };
            statusElements.Add(healthBarBg);

            // Create health bar foreground (red)
            var healthBarFg = new Rectangle
            {
                Height = BarHeight,
                Fill = _healthBarFill
            };
            statusElements.Add(healthBarFg);

            // Create energy bar background (black)
            var energyBarBg = new Rectangle
            {
                Height = BarHeight,
                Fill = Brushes.Black,
                Stroke = _barBorder,
                StrokeThickness = 0.5
            };
            statusElements.Add(energyBarBg);

            // Create energy bar foreground (yellow)
            var energyBarFg = new Rectangle
            {
                Height = BarHeight,
                Fill = _energyBarFill
            };
            statusElements.Add(energyBarFg);

            // Add food type indicators if enabled
            if (_showFoodType)
            {
                // Food material indicator (green for plant, red for meat)
                var foodTypeIcon = new Ellipse
                {
                    Width = IconSize,
                    Height = IconSize,
                    Fill = energy.IsPlantMaterial ? _plantIcon : _meatIcon
                };
                statusElements.Add(foodTypeIcon);

                // Diet type indicator (color based on digestion spectrum)
                var digestionIcon = new Ellipse
                {
                    Width = IconSize,
                    Height = IconSize,
                    Fill = Brushes.Yellow // Default, will be updated in render loop
                };
                statusElements.Add(digestionIcon);
            }

            // Add all elements to canvas
            foreach (var element in statusElements)
            {
                _canvas.Children.Add(element);

                // Set higher Z-index to ensure elements appear above organisms
                Canvas.SetZIndex(element, 5);
            }

            // Store references to the UI elements
            _entityStatusBars[entity] = statusElements.ToArray();
        }
    }
}
