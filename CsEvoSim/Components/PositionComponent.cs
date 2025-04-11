using CsEvoSim.Core;

namespace CsEvoSim.Components
{
    public class PositionComponent : IComponent
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PositionComponent(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
