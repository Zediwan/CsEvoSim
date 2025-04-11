using System;
using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Systems
{
    public class EnergySystem : ISystemWithSettings
    {
        private double _healthToSizeMultiplier = 10.0; // Health = Size * Multiplier
        private double _energyToSizeMultiplier = 15.0; // Energy = Size * Multiplier
        private double _baseMetabolicRate = 0.1;     // Base energy consumed per second
        private double _movementEnergyFactor = 0.5;  // How much movement increases energy consumption
        private double _healthLossRate = 0.5;        // Health lost per second when out of energy
        private double _environmentalSunlight = 1.0;  // Global sunlight level (affects photosynthesis)

        public string SettingsGroupName => "Energy";

        public IEnumerable<SystemSetting> GetSettings()
        {
            yield return SystemSetting.CreateNumeric(
                "HealthToSizeMultiplier",
                "Health-Size Multiplier",
                _healthToSizeMultiplier, 5.0, 30.0, 1.0,
                val => _healthToSizeMultiplier = val,
                "How much health an organism has per unit of size"
            );

            yield return SystemSetting.CreateNumeric(
                "EnergyToSizeMultiplier",
                "Energy-Size Multiplier",
                _energyToSizeMultiplier, 5.0, 30.0, 1.0,
                val => _energyToSizeMultiplier = val,
                "How much energy an organism has per unit of size"
            );

            yield return SystemSetting.CreateNumeric(
                "BaseMetabolicRate",
                "Base Metabolic Rate",
                _baseMetabolicRate, 0.01, 1.0, 0.01,
                val => _baseMetabolicRate = val,
                "How much energy is consumed per second at rest"
            );

            yield return SystemSetting.CreateNumeric(
                "MovementEnergyFactor",
                "Movement Energy Factor",
                _movementEnergyFactor, 0.1, 2.0, 0.1,
                val => _movementEnergyFactor = val,
                "How much movement increases energy consumption"
            );

            yield return SystemSetting.CreateNumeric(
                "HealthLossRate",
                "Starvation Rate",
                _healthLossRate, 0.1, 2.0, 0.1,
                val => _healthLossRate = val,
                "How much health is lost per second when out of energy"
            );

            yield return SystemSetting.CreateNumeric(
                "EnvironmentalSunlight",
                "Sunlight Level",
                _environmentalSunlight, 0.1, 2.0, 0.1,
                val => _environmentalSunlight = val,
                "Global sunlight level affecting photosynthesis (1.0 = normal)"
            );
        }

        public void Update(List<Entity> entities)
        {
            double frameTime = 1.0 / 60.0; // Assuming 60 FPS
            List<Entity> entitiesToRemove = new List<Entity>();

            foreach (var entity in entities)
            {
                var energy = entity.GetComponent<EnergyComponent>();
                var dna = entity.GetComponent<DNAComponent>();
                var position = entity.GetComponent<PositionComponent>();

                if (energy == null || dna == null) continue;

                // Update max values based on size
                energy.MaxHealth = dna.Size * _healthToSizeMultiplier;
                energy.MaxEnergy = dna.Size * _energyToSizeMultiplier;

                // Run photosynthesis for capable organisms
                energy.UpdatePhotosynthesis(frameTime, _environmentalSunlight);

                // Update digestion cooldown
                energy.UpdateDigestion(frameTime);

                // Calculate metabolic cost (size-based + movement)
                double metabolicCost = _baseMetabolicRate * dna.Size * frameTime;

                // Add movement cost if moving
                if (position != null && dna.MovementSpeed > 0.1)
                {
                    metabolicCost += dna.Size * dna.MovementSpeed * _movementEnergyFactor * frameTime;
                }

                // Apply metabolic cost
                energy.Energy -= metabolicCost;

                // Handle starvation
                if (energy.Energy <= 0)
                {
                    // Calculate starvation severity based on how negative energy is
                    double starvationSeverity = Math.Min(1.0, Math.Abs(energy.Energy / energy.MaxEnergy));

                    // Apply health loss
                    double healthLoss = _healthLossRate * frameTime * (1.0 + starvationSeverity);

                    energy.Energy = 0; // Clamp to zero
                    energy.Health -= healthLoss;

                    // Mark for removal if health reaches zero
                    if (energy.Health <= 0)
                    {
                        entitiesToRemove.Add(entity);
                    }
                }
            }

            // Remove dead entities
            foreach (var deadEntity in entitiesToRemove)
            {
                entities.Remove(deadEntity);
            }
        }
    }
}
