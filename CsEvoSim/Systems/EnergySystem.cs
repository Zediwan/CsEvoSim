// File: CsEvoSim/Systems/EnergySystem.cs
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
        private double _passiveEnergyRegenRate = 0.1; // Energy regenerated per second when not moving
        private double _healthLossRate = 0.5; // Health lost per second when out of energy
        private double _healthToEnergyRatio = 3.0; // How much 1 health point equals in energy units

        public string SettingsGroupName => "Energy";

        // Property to expose the ratio to other systems
        public double HealthToEnergyRatio => _healthToEnergyRatio;

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
                "PassiveEnergyRegenRate",
                "Energy Regen Rate",
                _passiveEnergyRegenRate, 0.0, 1.0, 0.05,
                val => _passiveEnergyRegenRate = val,
                "How much energy regenerates passively per second"
            );

            yield return SystemSetting.CreateNumeric(
                "HealthLossRate",
                "Health Loss Rate",
                _healthLossRate, 0.1, 2.0, 0.1,
                val => _healthLossRate = val,
                "How much health is lost per second when energy is depleted"
            );

            yield return SystemSetting.CreateNumeric(
                "HealthToEnergyRatio",
                "Health to Energy Ratio",
                _healthToEnergyRatio, 1.0, 10.0, 0.5,
                val => _healthToEnergyRatio = val,
                "How much energy 1 point of health equals (higher = health is more valuable)"
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

                if (energy == null || dna == null) continue;

                // Update max health/energy based on size and current multipliers
                energy.MaxHealth = dna.Size * _healthToSizeMultiplier;
                energy.MaxEnergy = dna.Size * _energyToSizeMultiplier;

                // Update the energy component's internal ratio value
                energy.HealthToEnergyRatio = _healthToEnergyRatio;

                // Cap current health/energy to max if needed
                energy.Health = Math.Min(energy.Health, energy.MaxHealth);
                energy.Energy = Math.Min(energy.Energy, energy.MaxEnergy);

                // Passive energy regeneration for non-moving organisms
                if (dna.MovementSpeed <= 0.1)
                {
                    energy.Energy += _passiveEnergyRegenRate * frameTime;
                    energy.Energy = Math.Min(energy.Energy, energy.MaxEnergy);
                }

                // Health loss when energy is depleted
                if (energy.Energy <= 0)
                {
                    energy.Health -= _healthLossRate * frameTime;

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
