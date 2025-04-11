using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CsEvoSim.Core;

namespace CsEvoSim.Components
{
    public enum MutationType
    {
        PointMutation,   // Single gene value change
        Deletion,        // Remove a gene segment
        Duplication,     // Duplicate a gene segment
        Inversion,       // Reverse a gene segment
        Translocation,   // Move a gene segment to a different position
        Insertion        // Insert a new random gene
    }

    public class DNAComponent : IComponent
    {
        // Genes list - direct access provided for certain operations
        public List<double> Genes { get; private set; }

        // Gene interpretation constants
        private const int SIZE_GENE = 0;
        private const int RED_GENE = 1;
        private const int GREEN_GENE = 2;
        private const int BLUE_GENE = 3;
        private const int MOVEMENT_GENE = 4;
        private const int PHOTOSYNTHESIS_GENE = 5;
        private const int DIGESTION_GENE = 6;
        private const int REPRODUCTION_THRESHOLD_GENE = 7; // New gene for reproduction threshold

        // Static random generator
        private static readonly Random rand = new();

        public DNAComponent(List<double> genes)
        {
            Genes = genes;

            // Ensure we have all required genes by padding with random values if needed
            while (Genes.Count < 8) // Updated: now 8 genes with reproduction threshold
            {
                Genes.Add(rand.NextDouble());
            }
        }

        // Physical traits
        public double Size => Lerp(5, 20, Genes[SIZE_GENE]);
        public byte Red => (byte)(Genes[RED_GENE] * 255);
        public byte Green => (byte)(Genes[GREEN_GENE] * 255);
        public byte Blue => (byte)(Genes[BLUE_GENE] * 255);
        public double MovementSpeed => Genes[MOVEMENT_GENE];

        // Metabolic traits
        public bool CanPhotosynthesize => Genes[PHOTOSYNTHESIS_GENE] > 0.7;
        public double PhotosynthesisEfficiency => Genes[PHOTOSYNTHESIS_GENE];
        public double DigestionSpectrum => Lerp(-1.0, 1.0, Genes[DIGESTION_GENE]);

        // Reproduction traits
        public double ReproductionThreshold => Lerp(0.6, 0.9, Genes[REPRODUCTION_THRESHOLD_GENE]);

        // Visual properties
        public Color Color => Color.FromRgb(Red, Green, Blue);

        // Create a clone with potential mutations
        public DNAComponent Reproduce(double mutationRate, Dictionary<MutationType, double> mutationWeights)
        {
            // Create a copy of the genes
            var childGenes = new List<double>(Genes);

            // Apply mutations based on mutation rate
            if (rand.NextDouble() < mutationRate)
            {
                // Select mutation type based on weights
                MutationType mutation = SelectMutationType(mutationWeights);
                ApplyMutation(childGenes, mutation);
            }

            return new DNAComponent(childGenes);
        }

        // Select a mutation type based on weights
        private MutationType SelectMutationType(Dictionary<MutationType, double> mutationWeights)
        {
            // Default to point mutation if no weights defined
            if (mutationWeights == null || mutationWeights.Count == 0)
                return MutationType.PointMutation;

            // Get total weight
            double totalWeight = mutationWeights.Values.Sum();

            // Select random value within total weight range
            double randomValue = rand.NextDouble() * totalWeight;
            double weightSum = 0;

            // Find the selected mutation type
            foreach (var pair in mutationWeights)
            {
                weightSum += pair.Value;
                if (randomValue <= weightSum)
                    return pair.Key;
            }

            // Fallback to point mutation
            return MutationType.PointMutation;
        }

        // Apply the specified mutation to the genes
        private void ApplyMutation(List<double> genes, MutationType mutationType)
        {
            switch (mutationType)
            {
                case MutationType.PointMutation:
                    ApplyPointMutation(genes);
                    break;

                case MutationType.Deletion:
                    ApplyDeletion(genes);
                    break;

                case MutationType.Duplication:
                    ApplyDuplication(genes);
                    break;

                case MutationType.Inversion:
                    ApplyInversion(genes);
                    break;

                case MutationType.Translocation:
                    ApplyTranslocation(genes);
                    break;

                case MutationType.Insertion:
                    ApplyInsertion(genes);
                    break;
            }

            // Ensure we maintain the essential genes (indices 0-7)
            EnsureEssentialGenes(genes);
        }

        // Ensure we have all essential genes (first 8)
        private void EnsureEssentialGenes(List<double> genes)
        {
            while (genes.Count < 8)
            {
                genes.Add(rand.NextDouble());
            }
        }

        // Point Mutation: Change a single gene value
        private void ApplyPointMutation(List<double> genes)
        {
            if (genes.Count == 0) return;

            // Select a random gene
            int index = rand.Next(genes.Count);

            // Mutate by adding a small random deviation
            double mutationStrength = rand.NextDouble() * 0.4 - 0.2; // -0.2 to +0.2
            genes[index] = Math.Clamp(genes[index] + mutationStrength, 0.0, 1.0);
        }

        // Deletion: Remove a gene segment
        private void ApplyDeletion(List<double> genes)
        {
            if (genes.Count <= 8) return; // Ensure we don't delete essential genes

            // Select a random segment for deletion (avoiding first 8 genes)
            int startIndex = rand.Next(8, genes.Count);
            int length = rand.Next(1, Math.Min(3, genes.Count - startIndex + 1));

            // Perform deletion
            genes.RemoveRange(startIndex, length);
        }

        // Duplication: Duplicate a gene segment
        private void ApplyDuplication(List<double> genes)
        {
            if (genes.Count == 0) return;

            // Select a random segment for duplication
            int startIndex = rand.Next(genes.Count);
            int length = rand.Next(1, Math.Min(3, genes.Count - startIndex + 1));

            // Copy the segment
            var segment = genes.GetRange(startIndex, length);

            // Insert the copy at a random position
            int insertIndex = rand.Next(genes.Count + 1);
            genes.InsertRange(insertIndex, segment);
        }

        // Inversion: Reverse a gene segment
        private void ApplyInversion(List<double> genes)
        {
            if (genes.Count < 2) return;

            // Select a random segment for inversion
            int startIndex = rand.Next(genes.Count - 1);
            int length = rand.Next(2, Math.Min(4, genes.Count - startIndex + 1));

            // Extract the segment
            var segment = genes.GetRange(startIndex, length);

            // Reverse it
            segment.Reverse();

            // Replace the original segment with the reversed one
            for (int i = 0; i < length; i++)
            {
                genes[startIndex + i] = segment[i];
            }
        }

        // Translocation: Move a gene segment to a different position
        private void ApplyTranslocation(List<double> genes)
        {
            if (genes.Count < 3) return;

            // Select a random segment for translocation
            int startIndex = rand.Next(genes.Count - 1);
            int length = rand.Next(1, Math.Min(3, genes.Count - startIndex + 1));

            // Extract the segment
            var segment = genes.GetRange(startIndex, length);
            genes.RemoveRange(startIndex, length);

            // Insert at a new position
            int insertIndex = rand.Next(genes.Count + 1);
            genes.InsertRange(insertIndex, segment);
        }

        // Insertion: Insert a new random gene
        private void ApplyInsertion(List<double> genes)
        {
            // Insert a random gene at a random position
            int insertIndex = rand.Next(genes.Count + 1);
            genes.Insert(insertIndex, rand.NextDouble());
        }

        private static double Lerp(double min, double max, double normalized)
        {
            return min + (max - min) * normalized;
        }

        public static DNAComponent Random()
        {
            var genes = new List<double>();

            for (int i = 0; i < 8; i++) // Updated: now 8 genes including reproduction threshold
                genes.Add(rand.NextDouble());

            return new DNAComponent(genes);
        }
    }
}
