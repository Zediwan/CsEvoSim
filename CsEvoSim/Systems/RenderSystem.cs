using System;
using System.Collections.Generic;
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
        private bool _showDebugInfo = false;

        // Track entities to be removed when they go off-screen
        private readonly List<Entity> _entitiesToRemove = new();

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
            _entitiesToRemove.Clear();
            double canvasWidth = _canvas.ActualWidth;
            double canvasHeight = _canvas.ActualHeight;

            foreach (var entity in entities)
            {
                var pos = entity.GetComponent<PositionComponent>();
                var dna = entity.GetComponent<DNAComponent>();
                if (pos == null || dna == null) continue;

                double radius = dna.Size / 2;

                // Create visual if missing
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

                // Position the shape accounting for size (center-based positioning)
                Canvas.SetLeft(shape, pos.X - radius);
                Canvas.SetTop(shape, pos.Y - radius);

                // Debug visualization when enabled
                if (_showDebugInfo)
                {
                    // Example: You could add movement vector indication or other debug visuals
                }
            }

            // Clean up visuals for removed entities
            foreach (var entity in _entitiesToRemove)
            {
                if (_entityVisuals.TryGetValue(entity, out var visual))
                {
                    _canvas.Children.Remove(visual);
                    _entityVisuals.Remove(entity);
                }
            }
        }
    }
}