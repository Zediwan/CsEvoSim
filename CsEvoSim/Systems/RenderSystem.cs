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

        // Track entities that exist in the simulation
        private HashSet<Entity> _currentEntities = new();

        // Colors for status bars
        private readonly SolidColorBrush _healthBarFill = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush _energyBarFill = new SolidColorBrush(Colors.Yellow);
        private readonly SolidColorBrush _barBorder = new SolidColorBrush(Colors.White);

        // Status bar configuration
        private const double BarHeight = 2;
        private const double BarSpacing = 1;
        private const double BarOffset = 5;

        public bool ShowDebugInfo
        {
            get => _showDebugInfo;
            set => _showDebugInfo = value;
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
                            CreateStatusBars(entity, entityRadius);
                        }

                        // Get status bars
                        var statusBars = _entityStatusBars[entity];
                        var healthBarBg = (Rectangle)statusBars[0];
                        var healthBarFg = (Rectangle)statusBars[1];
                        var energyBarBg = (Rectangle)statusBars[2];
                        var energyBarFg = (Rectangle)statusBars[3];

                        // Update status bar positions
                        double barWidth = entityRadius * 2;
                        double healthBarY = entityCenterY - entityRadius - BarOffset - BarHeight * 2 - BarSpacing;
                        double energyBarY = entityCenterY - entityRadius - BarOffset - BarHeight;

                        // Position health bar background
                        Canvas.SetLeft(healthBarBg, entityCenterX - entityRadius);
                        Canvas.SetTop(healthBarBg, healthBarY);
                        healthBarBg.Width = barWidth;

                        // Position and scale health bar foreground based on health percentage
                        Canvas.SetLeft(healthBarFg, entityCenterX - entityRadius);
                        Canvas.SetTop(healthBarFg, healthBarY);
                        healthBarFg.Width = barWidth * energy.HealthPercentage;

                        // Position energy bar background
                        Canvas.SetLeft(energyBarBg, entityCenterX - entityRadius);
                        Canvas.SetTop(energyBarBg, energyBarY);
                        energyBarBg.Width = barWidth;

                        // Position and scale energy bar foreground based on energy percentage
                        Canvas.SetLeft(energyBarFg, entityCenterX - entityRadius);
                        Canvas.SetTop(energyBarFg, energyBarY);
                        energyBarFg.Width = barWidth * energy.EnergyPercentage;

                        // Make bars visible
                        foreach (var bar in statusBars)
                        {
                            bar.Visibility = Visibility.Visible;
                        }

                        // Adjust organism opacity based on health
                        if (shape.Fill is SolidColorBrush colorBrush)
                        {
                            Color baseColor = dna.Color;
                            shape.Fill = new SolidColorBrush(baseColor);
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
                }
            }
        }

        private void CreateStatusBars(Entity entity, double radius)
        {
            // Create health bar background (black)
            var healthBarBg = new Rectangle
            {
                Height = BarHeight,
                Fill = Brushes.Black,
                Stroke = _barBorder,
                StrokeThickness = 0.5
            };

            // Create health bar foreground (red)
            var healthBarFg = new Rectangle
            {
                Height = BarHeight,
                Fill = _healthBarFill
            };

            // Create energy bar background (black)
            var energyBarBg = new Rectangle
            {
                Height = BarHeight,
                Fill = Brushes.Black,
                Stroke = _barBorder,
                StrokeThickness = 0.5
            };

            // Create energy bar foreground (yellow)
            var energyBarFg = new Rectangle
            {
                Height = BarHeight,
                Fill = _energyBarFill
            };

            // Add all bars to canvas
            _canvas.Children.Add(healthBarBg);
            _canvas.Children.Add(healthBarFg);
            _canvas.Children.Add(energyBarBg);
            _canvas.Children.Add(energyBarFg);

            // Set higher Z-index to ensure bars appear above organisms
            Canvas.SetZIndex(healthBarBg, 5);
            Canvas.SetZIndex(healthBarFg, 6);
            Canvas.SetZIndex(energyBarBg, 5);
            Canvas.SetZIndex(energyBarFg, 6);

            // Store references to the bars
            _entityStatusBars[entity] = new UIElement[] { healthBarBg, healthBarFg, energyBarBg, energyBarFg };
        }
    }
}
