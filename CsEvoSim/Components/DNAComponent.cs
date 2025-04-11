using System.Collections.Generic;
using System.Windows.Media;
using CsEvoSim.Core;

namespace CsEvoSim.Components
{
    public class DNAComponent : IComponent
    {
        public List<double> Genes { get; private set; }

        public DNAComponent(List<double> genes)
        {
            Genes = genes;
        }

        // Physical traits
        public double Size => Lerp(5, 20, Genes[0]); // index 0
        public byte Red => (byte)(Genes[1] * 255);   // index 1
        public byte Green => (byte)(Genes[2] * 255); // index 2
        public byte Blue => (byte)(Genes[3] * 255);  // index 3
        public double MovementSpeed => Genes[4];     // index 4: Movement speed factor (0.0 = no movement, 1.0 = maximum)

        // Metabolic traits
        public bool CanPhotosynthesize => Genes[5] > 0.7;   // index 5: photosynthesis ability threshold
        public double PhotosynthesisEfficiency => Genes[5]; // index 5: also used for efficiency (0.0-1.0)
        public double DigestionSpectrum => Lerp(-1.0, 1.0, Genes[6]);  // index 6: -1.0 herbivore to +1.0 carnivore

        // Visual properties
        public Color Color => Color.FromRgb(Red, Green, Blue);

        private static double Lerp(double min, double max, double normalized)
        {
            return min + (max - min) * normalized;
        }

        public static DNAComponent Random()
        {
            var genes = new List<double>();
            var rand = new System.Random();

            for (int i = 0; i < 7; i++) // Updated genome: Size + RGB + MovementSpeed + Photosynthesis + DigestionSpectrum
                genes.Add(rand.NextDouble());

            return new DNAComponent(genes);
        }
    }
}
