using System;
using System.Collections.Generic;
using System.Linq;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Systems
{
    public class FeedingSystem : ISystemWithSettings
    {
        // Settings
        private double _baseDetectionRange = 50.0;  // Base range for detecting prey
        private double _sizeRatioPredator = 1.2;    // How much bigger a predator must be to eat prey
        private double _maxDigestCooldown = 2.0;    // Time between feeding actions
        private double _digestionEfficiencyFactor = 1.0; // Global multiplier for digestion efficiency

        public string SettingsGroupName => "Feeding";

        public IEnumerable<SystemSetting> GetSettings()
        {
            yield return SystemSetting.CreateNumeric(
                "BaseDetectionRange",
                "Prey Detection Range",
                _baseDetectionRange, 10.0, 100.0, 5.0,
                val => _baseDetectionRange = val,
                "Base range at which organisms can detect potential food"
            );

            yield return SystemSetting.CreateNumeric(
                "SizeRatioPredator",
                "Predator Size Ratio",
                _sizeRatioPredator, 1.0, 3.0, 0.1,
                val => _sizeRatioPredator = val,
                "How much larger an organism must be to consume another"
            );

            yield return SystemSetting.CreateNumeric(
                "MaxDigestCooldown",
                "Digest Cooldown Time",
                _maxDigestCooldown, 0.5, 10.0, 0.5,
                val => _maxDigestCooldown = val,
                "Time in seconds between feeding actions"
            );

            yield return SystemSetting.CreateNumeric(
                "DigestionEfficiencyFactor",
                "Digestion Efficiency",
                _digestionEfficiencyFactor, 0.1, 3.0, 0.1,
                val => _digestionEfficiencyFactor = val,
                "Global multiplier for digestion efficiency"
            );
        }

        public void Update(List<Entity> entities)
        {
            // Entities to be removed after being consumed
            List<Entity> consumedEntities = new List<Entity>();

            // Process feeding interactions
            foreach (var predator in entities)
            {
                // Skip if already marked as consumed
                if (consumedEntities.Contains(predator))
                    continue;

                var predatorEnergy = predator.GetComponent<EnergyComponent>();
                var predatorPos = predator.GetComponent<PositionComponent>();
                var predatorDNA = predator.GetComponent<DNAComponent>();

                if (predatorEnergy == null || predatorPos == null || predatorDNA == null)
                    continue;

                // Skip if on cooldown
                if (predatorEnergy.DigestCooldown > 0)
                    continue;

                // Skip if no digestion capabilities
                if (predatorEnergy.GetPlantDigestionEfficiency() <= 0 &&
                    predatorEnergy.GetMeatDigestionEfficiency() <= 0)
                    continue;

                // Calculate detection range based on size
                double detectionRange = _baseDetectionRange * (0.5 + predatorDNA.Size / 20.0);

                // Find potential prey
                var potentialPrey = entities
                    .Where(prey => prey != predator && !consumedEntities.Contains(prey))
                    .Select(prey => {
                        var preyPos = prey.GetComponent<PositionComponent>();
                        var preyEnergy = prey.GetComponent<EnergyComponent>();
                        var preyDNA = prey.GetComponent<DNAComponent>();

                        return new
                        {
                            Entity = prey,
                            Position = preyPos,
                            Energy = preyEnergy,
                            DNA = preyDNA,
                            Distance = preyPos != null && predatorPos != null ?
                                Math.Sqrt(
                                    Math.Pow(predatorPos.X - preyPos.X, 2) +
                                    Math.Pow(predatorPos.Y - preyPos.Y, 2)
                                ) : double.MaxValue
                        };
                    })
                    .Where(p =>
                        p.Position != null &&
                        p.Energy != null &&
                        p.DNA != null &&
                        p.Distance <= detectionRange
                    )
                    .OrderBy(p => p.Distance)
                    .Where(p => {
                        // Check if predator can digest this prey
                        bool canDigest = p.Energy.IsPlantMaterial ?
                            predatorEnergy.GetPlantDigestionEfficiency() > 0 :
                            predatorEnergy.GetMeatDigestionEfficiency() > 0;

                        // Check if predator is big enough
                        bool isBigEnough = predatorDNA.Size >= p.DNA.Size * _sizeRatioPredator;

                        return canDigest && isBigEnough;
                    })
                    .ToList();

                // Consume the closest valid prey
                if (potentialPrey.Any())
                {
                    var prey = potentialPrey.First();

                    // Consume and gain/lose energy based on digestion efficiency
                    predatorEnergy.Consume(prey.Energy);

                    // Mark as consumed
                    consumedEntities.Add(prey.Entity);
                }
            }

            // Remove consumed entities
            foreach (var consumed in consumedEntities)
            {
                entities.Remove(consumed);
            }
        }
    }
}
