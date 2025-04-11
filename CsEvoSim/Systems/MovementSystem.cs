using System;
using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;
using SimplexNoise;

namespace CsEvoSim.Systems
{
    public class MovementSystem : ISystemWithSettings
    {
        private readonly Random _random = new();
        private double _movementScale = 1.5; // base movement speed
        private double _noiseStep = 0.01;    // how fast noise changes
        private double _speedNoiseStep = 0.015; // slightly different rate for speed variation
        private double _noiseTime = 0.0;
        private double _speedNoiseTime = 0.0;

        // Properties for settings
        public double MovementScale
        {
            get => _movementScale;
            set => _movementScale = value;
        }

        public double NoiseStep
        {
            get => _noiseStep;
            set => _noiseStep = value;
        }

        public double SpeedNoiseStep
        {
            get => _speedNoiseStep;
            set => _speedNoiseStep = value;
        }

        public string SettingsGroupName => "Movement";

        public MovementSystem()
        {
            // Initialize SimplexNoise with a random seed
            Noise.Seed = _random.Next();
        }

        public IEnumerable<SystemSetting> GetSettings()
        {
            yield return SystemSetting.CreateNumeric(
                "MovementScale",
                "Base Movement Speed",
                _movementScale, 0.5, 5.0, 0.1,
                val => _movementScale = val,
                "Base speed multiplier for organism movement"
            );

            yield return SystemSetting.CreateNumeric(
                "NoiseStep",
                "Direction Change Rate",
                _noiseStep, 0.001, 0.05, 0.001,
                val => _noiseStep = val,
                "How quickly organisms change direction (higher = more erratic)"
            );

            yield return SystemSetting.CreateNumeric(
                "SpeedNoiseStep",
                "Speed Variation Rate",
                _speedNoiseStep, 0.001, 0.05, 0.001,
                val => _speedNoiseStep = val,
                "How quickly organism speed varies (higher = more variable speed)"
            );
        }

        public void Update(List<Entity> entities)
        {
            // Existing implementation
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
