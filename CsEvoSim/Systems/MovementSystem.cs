using System;
using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;
using SimplexNoise;

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

        public MovementSystem()
        {
            // Initialize SimplexNoise with a random seed
            Noise.Seed = _random.Next();
        }

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

                // Use entity hash code as a unique identifier for consistent noise per entity
                int entityId = Math.Abs(entity.GetHashCode()) % 10000;

                // Use SimplexNoise for directional changes (2D noise for better variation)
                double directionNoise = Noise.CalcPixel2D(
                    (int)(_noiseTime * 100),
                    entityId,
                    0.005f);

                // Map noise output from [-1,1] to [0,2π] for angle
                double angle = (directionNoise + 1) * Math.PI;

                // Use SimplexNoise for speed variation (with different coordinates)
                double speedNoise = Noise.CalcPixel2D(
                    (int)(_speedNoiseTime * 100),
                    entityId + 5000, // offset to get different noise pattern
                    0.005f);

                // Map from [-1,1] to [0.2,1.0] to avoid too slow movement
                double speedFactor = (speedNoise + 1) * 0.4 + 0.2;

                // Scale movement by the DNA-defined maximum movement speed and the noise factor
                double maxSpeed = _movementScale * dna.MovementSpeed;
                double actualSpeed = maxSpeed * speedFactor;

                double dx = Math.Cos(angle) * actualSpeed;
                double dy = Math.Sin(angle) * actualSpeed;

                position.X += dx;
                position.Y += dy;
            }
        }
    }
}
