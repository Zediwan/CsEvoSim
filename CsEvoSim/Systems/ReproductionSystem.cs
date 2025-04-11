using System;
using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;
using CsEvoSim.Utils;

namespace CsEvoSim.Systems
{
    public class ReproductionSystem : ISystemWithSettings
    {
        private double _baseMutationRate = 0.2;       // Base chance of mutation
        private double _maxOrganismCount = 200;       // Maximum organisms in world
        private double _populationScaleFactor = 0.8;  // How much population affects reproduction
        private bool _requireEnergyThreshold = true;  // Whether reproduction requires reaching threshold energy

        // Mutation type weights
        private Dictionary<MutationType, double> _mutationWeights = new Dictionary<MutationType, double>
        {
            { MutationType.PointMutation, 0.6 }, // Most common
            { MutationType.Deletion, 0.1 },
            { MutationType.Duplication, 0.1 },
            { MutationType.Inversion, 0.1 },
            { MutationType.Translocation, 0.05 },
            { MutationType.Insertion, 0.05 }
        };

        // Cached world dimensions
        private double _worldWidth;
        private double _worldHeight;

        public string SettingsGroupName => "Reproduction";

        public ReproductionSystem(double worldWidth, double worldHeight)
        {
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
        }

        public void SetWorldDimensions(double width, double height)
        {
            _worldWidth = width;
            _worldHeight = height;
        }

        public IEnumerable<SystemSetting> GetSettings()
        {
            yield return SystemSetting.CreateNumeric(
                "BaseMutationRate",
                "Base Mutation Rate",
                _baseMutationRate, 0.0, 1.0, 0.05,
                val => _baseMutationRate = val,
                "Base probability of mutation per reproduction (0.0-1.0)"
            );

            yield return SystemSetting.CreateNumeric(
                "MaxOrganismCount",
                "Max Population",
                _maxOrganismCount, 50, 1000, 50,
                val => _maxOrganismCount = val,
                "Maximum number of organisms allowed in the world"
            );

            yield return SystemSetting.CreateNumeric(
                "PopulationScaleFactor",
                "Population Effect",
                _populationScaleFactor, 0.0, 1.0, 0.1,
                val => _populationScaleFactor = val,
                "How much population density affects reproduction (0.0-1.0)"
            );

            yield return SystemSetting.CreateBoolean(
                "RequireEnergyThreshold",
                "Energy Threshold Required",
                _requireEnergyThreshold,
                val => _requireEnergyThreshold = val,
                "Whether reproduction requires reaching energy threshold"
            );

            // Mutation type weights
            foreach (MutationType mutationType in Enum.GetValues(typeof(MutationType)))
            {
                double weight = _mutationWeights.ContainsKey(mutationType) ? _mutationWeights[mutationType] : 0.0;

                yield return SystemSetting.CreateNumeric(
                    $"Weight{mutationType}",
                    $"{mutationType} Weight",
                    weight, 0.0, 1.0, 0.05,
                    val => _mutationWeights[mutationType] = val,
                    $"Relative probability of {mutationType} mutations"
                );
            }
        }

        public void Update(List<Entity> entities)
        {
            // Skip reproduction if at max capacity
            if (entities.Count >= _maxOrganismCount)
                return;

            // Population density factor (more organisms = less reproduction)
            double populationFactor = 1.0 - (entities.Count / _maxOrganismCount * _populationScaleFactor);
            if (populationFactor <= 0) return;

            // List to hold new offspring
            List<Entity> offspring = new List<Entity>();
            double frameTime = 1.0 / 60.0; // Assuming 60FPS

            // Process each organism for potential reproduction
            foreach (var entity in entities)
            {
                var dna = entity.GetComponent<DNAComponent>();
                var energy = entity.GetComponent<EnergyComponent>();
                var reproduction = entity.GetComponent<ReproductionComponent>();

                if (dna == null || energy == null || reproduction == null)
                    continue;

                // Update reproduction cooldown
                reproduction.Update(frameTime);

                // Check if reproduction is possible
                if (!reproduction.CanReproduce)
                    continue;

                // Energy threshold check
                double energyThreshold = energy.MaxEnergy * dna.ReproductionThreshold;
                if (_requireEnergyThreshold && energy.Energy < energyThreshold)
                    continue;

                // Randomize reproduction chance based on population factor
                if (new Random().NextDouble() > populationFactor * 0.1) // 10% chance max, scaled by population
                    continue;

                // Consume energy for reproduction
                double reproductionCost = energy.MaxEnergy * 0.3; // 30% of max energy
                energy.Energy -= reproductionCost;

                // Reset reproduction cooldown
                reproduction.ResetCooldown();

                // Create offspring 
                var child = OrganismFactory.Reproduce(entity, _baseMutationRate, _mutationWeights,
                                                     _worldWidth, _worldHeight);
                if (child != null)
                {
                    offspring.Add(child);
                }
            }

            // Add all offspring to the main entities list
            foreach (var child in offspring)
            {
                entities.Add(child);
            }
        }
    }
}
