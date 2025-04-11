using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Systems
{
    public class RenderSystem : ISystem
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<Entity, Ellipse> _entityVisuals = new();

        public RenderSystem(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var pos = entity.GetComponent<PositionComponent>();
                var dna = entity.GetComponent<DNAComponent>();
                if (pos == null || dna == null) continue;

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
                Canvas.SetLeft(shape, pos.X);
                Canvas.SetTop(shape, pos.Y);
            }
        }
    }
}
