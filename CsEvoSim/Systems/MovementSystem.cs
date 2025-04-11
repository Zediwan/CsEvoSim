using System;
using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Systems
{
    public class MovementSystem : ISystem
    {
        private readonly Random _random = new();
        private readonly double _movementScale = 1.5; // base movement speed
        private readonly double _noiseStep = 0.01;    // how fast noise changes
        private readonly double _speedNoiseStep = 0.015; // slightly different rate for speed variation
        private double _noiseTime = 0.0;
        private double _speedNoiseTime = 0.0;

        public void Update(List<Entity> entities)
        {
            _noiseTime += _noiseStep;
            _speedNoiseTime += _speedNoiseStep;

            foreach (var entity in entities)
            {
                var position = entity.GetComponent<PositionComponent>();
                var dna = entity.GetComponent<DNAComponent>();

                if (position == null || dna == null) continue;

                // Skip movement for entities with zero movement speed
                if (dna.MovementSpeed <= 0.0) continue;

                // Use Perlin noise to get smooth directional change
                double angle = PerlinNoise(_noiseTime + entity.GetHashCode() * 0.1) * Math.PI * 2;

                // Use Perlin noise for speed variation as well
                double speedNoiseFactor = PerlinNoise(_speedNoiseTime + entity.GetHashCode() * 0.13);

                // Scale movement by the DNA-defined maximum movement speed and the noise factor
                double maxSpeed = _movementScale * dna.MovementSpeed;
                double actualSpeed = maxSpeed * speedNoiseFactor;

                double dx = Math.Cos(angle) * actualSpeed;
                double dy = Math.Sin(angle) * actualSpeed;

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
