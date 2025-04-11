using CsEvoSim.Core;
using System.Collections.Generic;

namespace CsEvoSim.Components
{
    public class DNAComponent : IComponent
    {
        public List<double> Genes { get; private set; }

        public DNAComponent(List<double> genes)
        {
            Genes = genes;
        }

        // Gene interpretation (based on index)
        public double Size => Lerp(5, 20, Genes[0]); // index 0
        public byte Red => (byte)(Genes[1] * 255);   // index 1
        public byte Green => (byte)(Genes[2] * 255); // index 2
        public byte Blue => (byte)(Genes[3] * 255);  // index 3
        public double MovementSpeed => Genes[4];     // index 4: Movement speed factor (0.0 = no movement, 1.0 = maximum)

        public System.Windows.Media.Color Color =>
            System.Windows.Media.Color.FromRgb(Red, Green, Blue);

        private static double Lerp(double min, double max, double normalized)
        {
            return min + (max - min) * normalized;
        }

        public static DNAComponent Random()
        {
            var genes = new List<double>();
            var rand = new System.Random();

            for (int i = 0; i < 5; i++) // updated genome: Size + RGB + MovementSpeed
                genes.Add(rand.NextDouble());

            return new DNAComponent(genes);
        }
    }
}
