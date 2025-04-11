using CsEvoSim.Core;

namespace CsEvoSim.Components
{
    public class EnergyComponent : IComponent
    {
        public double Energy { get; set; }

        public EnergyComponent(double initial)
        {
            Energy = initial;
        }
    }
}
