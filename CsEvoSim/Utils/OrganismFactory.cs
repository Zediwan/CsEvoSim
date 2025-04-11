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

            entity.AddComponent(new PositionComponent(x, y));
            entity.AddComponent(DNAComponent.Random());

            return entity;
        }
    }
}
