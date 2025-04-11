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
                dna.Genes[i + 1] = adjustedGenes[i + 1];
            }
        }
    }
}
