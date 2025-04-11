using CsEvoSim.Core;
using CsEvoSim.Components;

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

            var dna = DNAComponent.Random();
            entity.AddComponent(new PositionComponent(x, y));
            entity.AddComponent(dna);

            // Calculate initial health and energy based on size
            double maxHealth = dna.Size * 10.0; // Base health-size factor
            double maxEnergy = dna.Size * 15.0; // Base energy-size factor
            entity.AddComponent(new EnergyComponent(maxHealth, maxEnergy));

            return entity;
        }
    }
}
