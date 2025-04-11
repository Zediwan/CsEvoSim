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
        public const int SIZE_GENE = 0;             // CRITICAL - organism dies without this
        public const int RED_GENE = 1;              // Non-critical - defaults to 0
        public const int GREEN_GENE = 2;            // Non-critical - defaults to 0
        public const int BLUE_GENE = 3;             // Non-critical - defaults to 0
        public const int MOVEMENT_GENE = 4;         // Non-critical - defaults to 0
        public const int PHOTOSYNTHESIS_GENE = 5;   // Non-critical - defaults to false
        public const int DIGESTION_GENE = 6;        // CRITICAL - organism dies without this
        public const int REPRODUCTION_THRESHOLD_GENE = 7; // Semi-critical - organism becomes infertile

        // Static random generator
        private static readonly Random rand = new();

        public DNAComponent(List<double> genes)
        {
            Genes = genes ?? new List<double>();
        }

        // Physical traits - note some can cause death if missing
        public double Size => HasGene(SIZE_GENE) ? Lerp(5, 20, Genes[SIZE_GENE]) : 0;
        public byte Red => HasGene(RED_GENE) ? (byte)(Genes[RED_GENE] * 255) : (byte)0;
        public byte Green => HasGene(GREEN_GENE) ? (byte)(Genes[GREEN_GENE] * 255) : (byte)0;
        public byte Blue => HasGene(BLUE_GENE) ? (byte)(Genes[BLUE_GENE] * 255) : (byte)0;
        public double MovementSpeed => HasGene(MOVEMENT_GENE) ? Genes[MOVEMENT_GENE] : 0.0;

        // Metabolic traits
        public bool CanPhotosynthesize => HasGene(PHOTOSYNTHESIS_GENE) && Genes[PHOTOSYNTHESIS_GENE] > 0.7;
        public double PhotosynthesisEfficiency => HasGene(PHOTOSYNTHESIS_GENE) ? Genes[PHOTOSYNTHESIS_GENE] : 0.0;
        public double DigestionSpectrum => HasGene(DIGESTION_GENE) ? Lerp(-1.0, 1.0, Genes[DIGESTION_GENE]) : 0.0;

        // Reproduction traits
        public double ReproductionThreshold => HasGene(REPRODUCTION_THRESHOLD_GENE) ?
            Lerp(0.6, 0.9, Genes[REPRODUCTION_THRESHOLD_GENE]) : 1.0; // Infertile if missing

        // Visual properties
        public Color Color => Color.FromRgb(Red, Green, Blue);

        // Viability check - organism can only live if it has all critical genes
        public bool IsViable => HasGene(SIZE_GENE) && Size > 0 && HasGene(DIGESTION_GENE);

        // Fertility check - organism can only reproduce if it has reproduction gene
        public bool IsFertile => HasGene(REPRODUCTION_THRESHOLD_GENE);

        // Helper method to check if a specific gene exists
        public bool HasGene(int index) => index < Genes.Count && index >= 0;

        // Create a clone with potential mutations
        public DNAComponent Reproduce(double mutationRate, Dictionary<MutationType, double> mutationWeights)
        {
            if (!IsFertile)
                return null; // Infertile organisms can't reproduce

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

            // No automatic fixing of missing genes - let natural selection work
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

        // Deletion: Remove a gene segment - can delete any gene now
        private void ApplyDeletion(List<double> genes)
        {
            if (genes.Count <= 1) return; // Need at least one gene to delete

            // Select random gene segment
            int startIndex = rand.Next(genes.Count);
            int maxLength = Math.Min(3, genes.Count - startIndex);
            int length = maxLength > 0 ? rand.Next(1, maxLength + 1) : 1;

            // Perform deletion - can delete any gene now
            genes.RemoveRange(startIndex, length);
        }

        // Duplication: Duplicate a gene segment
        private void ApplyDuplication(List<double> genes)
        {
            if (genes.Count == 0) return;

            // Select a random segment for duplication
            int startIndex = rand.Next(genes.Count);
            int maxLength = Math.Min(3, genes.Count - startIndex);
            int length = maxLength > 0 ? rand.Next(1, maxLength + 1) : 1;

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
            int maxLength = Math.Min(4, genes.Count - startIndex);
            int length = maxLength >= 2 ? rand.Next(2, maxLength + 1) : 2;

            if (startIndex + length > genes.Count) return;

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
            int maxLength = Math.Min(3, genes.Count - startIndex);
            int length = maxLength > 0 ? rand.Next(1, maxLength + 1) : 1;

            if (startIndex + length > genes.Count) return;

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

            for (int i = 0; i < 8; i++) // Create all essential genes for initial population
                genes.Add(rand.NextDouble());

            return new DNAComponent(genes);
        }
    }
}
