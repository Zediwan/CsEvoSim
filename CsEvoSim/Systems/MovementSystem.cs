using System;
using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Systems
{
    public class MovementSystem : ISystem
    {
        private readonly Random _random = new();
        private readonly double _movementScale = 1.5; // max movement speed
        private readonly double _noiseStep = 0.01;    // how fast noise changes
        private double _noiseTime = 0.0;

        public void Update(List<Entity> entities)
        {
            _noiseTime += _noiseStep;

            foreach (var entity in entities)
            {
                var position = entity.GetComponent<PositionComponent>();
                if (position == null) continue;

                // Use Perlin noise to get smooth directional change
                double angle = PerlinNoise(_noiseTime + entity.GetHashCode() * 0.1) * Math.PI * 2;
                double dx = Math.Cos(angle) * _movementScale;
                double dy = Math.Sin(angle) * _movementScale;

                position.X += dx;
                position.Y += dy;
            }
        }

        // Simple Perlin noise approximation using sine curves (for demo)
        private static double PerlinNoise(double t)
        {
            return 0.5 + 0.5 * Math.Sin(t); // smooth oscillation from 0 to 1
        }
    }
}
