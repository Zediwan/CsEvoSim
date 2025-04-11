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
        private double _maxOverflowPercent = 10.0; // How far beyond screen edges organisms can go (%)
        private double _energyToMoveCostMultiplier = 0.2; // Base energy cost multiplier for movement
        private double _sizeCostMultiplier = 0.1; // How much size increases energy cost
        private double _starvingSpeedFactor = 0.5; // Speed multiplier when starving

        // Cached canvas dimensions
        private double _canvasWidth;
        private double _canvasHeight;

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

        public double MaxOverflowPercent
        {
            get => _maxOverflowPercent;
            set => _maxOverflowPercent = value;
        }

        public double EnergyToMoveCostMultiplier
        {
            get => _energyToMoveCostMultiplier;
            set => _energyToMoveCostMultiplier = value;
        }

        public double SizeCostMultiplier
        {
            get => _sizeCostMultiplier;
            set => _sizeCostMultiplier = value;
        }

        public double StarvingSpeedFactor
        {
            get => _starvingSpeedFactor;
            set => _starvingSpeedFactor = value;
        }

        public string SettingsGroupName => "Movement";

        public MovementSystem()
        {
            // Initialize SimplexNoise with a random seed
            Noise.Seed = _random.Next();
        }

        // Method to update canvas dimensions
        public void SetCanvasDimensions(double width, double height)
        {
            _canvasWidth = width;
            _canvasHeight = height;
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

            yield return SystemSetting.CreateNumeric(
                "MaxOverflowPercent",
                "Max Screen Overflow %",
                _maxOverflowPercent, 0.0, 30.0, 1.0,
                val => _maxOverflowPercent = val,
                "Maximum percentage beyond screen edges that organisms can travel"
            );

            yield return SystemSetting.CreateNumeric(
                "EnergyToMoveCostMultiplier",
                "Movement Energy Cost",
                _energyToMoveCostMultiplier, 0.05, 1.0, 0.05,
                val => _energyToMoveCostMultiplier = val,
                "Base energy cost for movement"
            );

            yield return SystemSetting.CreateNumeric(
                "SizeCostMultiplier",
                "Size Energy Cost Factor",
                _sizeCostMultiplier, 0.01, 0.5, 0.01,
                val => _sizeCostMultiplier = val,
                "How much size increases movement energy cost"
            );

            yield return SystemSetting.CreateNumeric(
                "StarvingSpeedFactor",
                "Starving Speed Factor",
                _starvingSpeedFactor, 0.1, 1.0, 0.1,
                val => _starvingSpeedFactor = val,
                "Movement speed factor when organism is starving (0.1-1.0)"
            );
        }

        public void Update(List<Entity> entities)
        {
            _noiseTime += _noiseStep;
            _speedNoiseTime += _speedNoiseStep;

            // Calculate boundary limits with overflow
            double overflowX = _canvasWidth * (_maxOverflowPercent / 100.0);
            double overflowY = _canvasHeight * (_maxOverflowPercent / 100.0);

            // Define boundaries
            double minX = -overflowX;
            double maxX = _canvasWidth + overflowX;
            double minY = -overflowY;
            double maxY = _canvasHeight + overflowY;

            foreach (var entity in entities)
            {
                var position = entity.GetComponent<PositionComponent>();
                var dna = entity.GetComponent<DNAComponent>();
                var energy = entity.GetComponent<EnergyComponent>();

                if (position == null || dna == null) continue;

                // Skip movement for entities with zero movement speed
                if (dna.MovementSpeed <= 0.0) continue;

                // Check if starving - plants that can photosynthesize always move at full speed
                bool isStarving = energy != null && energy.Energy <= 0 && !energy.CanPhotosynthesize;
                double starvationFactor = isStarving ? _starvingSpeedFactor : 1.0;

                // Get entity's randomized direction
                int entityId = Math.Abs(entity.GetHashCode()) % 10000;

                double directionNoise = Noise.CalcPixel2D(
                    (int)(_noiseTime * 100),
                    entityId,
                    0.005f);

                // Map noise output from [-1,1] to [0,2π] for angle
                double angle = (directionNoise + 1) * Math.PI;

                // Use SimplexNoise for speed variation
                double speedNoise = Noise.CalcPixel2D(
                    (int)(_speedNoiseTime * 100),
                    entityId + 5000,
                    0.005f);

                // Map from [-1,1] to [0.2,1.0] 
                double speedFactor = (speedNoise + 1) * 0.4 + 0.2;

                // Calculate final speed with all factors
                double maxSpeed = _movementScale * dna.MovementSpeed;
                double actualSpeed = maxSpeed * speedFactor * starvationFactor;

                // Calculate movement delta
                double dx = Math.Cos(angle) * actualSpeed;
                double dy = Math.Sin(angle) * actualSpeed;

                // Calculate energy cost
                if (energy != null && !isStarving)  // Only apply energy cost if not starving
                {
                    // Calculate movement cost
                    double movementCost = _energyToMoveCostMultiplier *
                                         actualSpeed *
                                         (1.0 + dna.Size * _sizeCostMultiplier);

                    // Apply cost
                    energy.Energy -= movementCost;
                    if (energy.Energy < 0)
                        energy.Energy = 0;
                }

                // Calculate new position
                double newX = position.X + dx;
                double newY = position.Y + dy;

                // Apply boundary constraints
                double radius = dna.Size / 2;
                newX = Math.Max(minX + radius, Math.Min(maxX - radius, newX));
                newY = Math.Max(minY + radius, Math.Min(maxY - radius, newY));

                // Update position
                position.X = newX;
                position.Y = newY;
            }
        }
    }
}
