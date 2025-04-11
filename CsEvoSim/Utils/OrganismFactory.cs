using System;
using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Utils
{
    public static class OrganismFactory
    {
        private static readonly Random rand = new();

        public static Entity CreateRandomOrganism(double maxX, double maxY)
        {
            var entity = new Entity();
            double x = rand.NextDouble() * maxX;
            double y = rand.NextDouble() * maxY;

            // Generate random DNA
            var dna = DNAComponent.Random();

            // Adjust colors based on traits for visual identification
            AdjustOrganismColors(dna);

            entity.AddComponent(new PositionComponent(x, y));
            entity.AddComponent(dna);

            // Calculate initial health and energy based on size
            double maxHealth = dna.Size * 10.0;
            double maxEnergy = dna.Size * 15.0;

            // Create energy component with traits from DNA
            var energyComponent = new EnergyComponent(maxHealth, maxEnergy)
            {
                CanPhotosynthesize = dna.CanPhotosynthesize,
                PhotosynthesisEfficiency = dna.PhotosynthesisEfficiency,
                DigestionSpectrum = dna.DigestionSpectrum,
                MaxDigestCooldown = 2.0 + rand.NextDouble() // Random variation in digest time
            };

            entity.AddComponent(energyComponent);

            // Add reproduction component
            var reproductionComponent = new ReproductionComponent(rand.NextDouble() * 5.0) // Random initial cooldown
            {
                MaxReproductionCooldown = 8.0 + rand.NextDouble() * 4.0 // 8-12 seconds between reproductions
            };
            entity.AddComponent(reproductionComponent);

            return entity;
        }

        public static Entity Reproduce(Entity parent, double mutationRate,
            Dictionary<MutationType, double> mutationWeights, double maxX, double maxY)
        {
            // Get parent components
            var parentDNA = parent.GetComponent<DNAComponent>();
            var parentPos = parent.GetComponent<PositionComponent>();

            if (parentDNA == null || parentPos == null)
                return null;

            // Check if parent is fertile before reproduction
            if (!parentDNA.IsFertile)
                return null;

            // Reproduce DNA with potential mutations
            var childDNA = parentDNA.Reproduce(mutationRate, mutationWeights);

            // If reproduction failed or child DNA is not viable, no offspring is produced
            if (childDNA == null || !childDNA.IsViable)
                return null;

            // Create and configure the new entity...
            var entity = new Entity();

            // Position near parent
            double offsetDistance = parentDNA.Size * 0.75;
            double offsetAngle = rand.NextDouble() * Math.PI * 2;
            double x = parentPos.X + Math.Cos(offsetAngle) * offsetDistance;
            double y = parentPos.Y + Math.Sin(offsetAngle) * offsetDistance;

            // Constrain position within world bounds
            x = Math.Clamp(x, 0, maxX);
            y = Math.Clamp(y, 0, maxY);

            // Add components
            entity.AddComponent(new PositionComponent(x, y));
            entity.AddComponent(childDNA);

            // Create energy component with traits from DNA
            double maxHealth = childDNA.Size * 10.0;
            double maxEnergy = childDNA.Size * 15.0;

            // Child starts with 50% of max health and energy
            var energyComponent = new EnergyComponent(maxHealth, maxEnergy)
            {
                Health = maxHealth * 0.5,
                Energy = maxEnergy * 0.5,
                CanPhotosynthesize = childDNA.CanPhotosynthesize,
                PhotosynthesisEfficiency = childDNA.PhotosynthesisEfficiency,
                DigestionSpectrum = childDNA.DigestionSpectrum,
                MaxDigestCooldown = 2.0 + rand.NextDouble()
            };
            entity.AddComponent(energyComponent);

            // Add reproduction component with full cooldown
            var reproductionComponent = new ReproductionComponent(10.0) // Start with full cooldown
            {
                MaxReproductionCooldown = 8.0 + rand.NextDouble() * 4.0 // 8-12 seconds
            };
            entity.AddComponent(reproductionComponent);

            // Apply color adjustments
            AdjustOrganismColors(childDNA);

            return entity;
        }

        // Helper method to adjust organism colors based on their traits
        private static void AdjustOrganismColors(DNAComponent dna)
        {
            List<double> adjustedGenes = new List<double>(dna.Genes);

            // Make photosynethsizing organisms more green
            if (dna.CanPhotosynthesize)
            {
                // Reduce red and blue, increase green
                adjustedGenes[1] *= 0.6; // Less red
                adjustedGenes[2] = 0.6 + (adjustedGenes[2] * 0.4); // More green
                adjustedGenes[3] *= 0.6; // Less blue
            }
            // Make carnivores more red
            else if (dna.DigestionSpectrum > 0.5)
            {
                // Increase red, reduce green and blue
                adjustedGenes[1] = 0.6 + (adjustedGenes[1] * 0.4); // More red
                adjustedGenes[2] *= 0.7; // Less green
                adjustedGenes[3] *= 0.7; // Less blue
            }
            // Make herbivores more colorful/diverse
            else if (dna.DigestionSpectrum < -0.5)
            {
                // Brighten all colors for variety
                adjustedGenes[1] = 0.3 + (adjustedGenes[1] * 0.7);
                adjustedGenes[2] = 0.3 + (adjustedGenes[2] * 0.7);
                adjustedGenes[3] = 0.3 + (adjustedGenes[3] * 0.7);
            }

            // Replace genes with adjusted ones
            for (int i = 0; i < 3; i++)
            {
                if (i + 1 < dna.Genes.Count)
                    dna.Genes[i + 1] = adjustedGenes[i + 1];
            }
        }
    }
}
