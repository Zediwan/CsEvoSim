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
                if (pos == null) continue;

                if (!_entityVisuals.ContainsKey(entity))
                {
                    var ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.CadetBlue
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
