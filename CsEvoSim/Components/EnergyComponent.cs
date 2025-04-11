using CsEvoSim.Core;

namespace CsEvoSim.Components
{
    public class EnergyComponent : IComponent
    {
        // Current values
        public double Health { get; set; }
        public double Energy { get; set; }

        // Maximum values
        public double MaxHealth { get; set; }
        public double MaxEnergy { get; set; }

        // Conversion ratio between health and energy
        public double HealthToEnergyRatio { get; set; } = 3.0; // Default: 1 health = 3 energy

        public EnergyComponent(double maxHealth, double maxEnergy)
        {
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;

            // Start with full health and energy
            Health = MaxHealth;
            Energy = MaxEnergy;
        }

        // Health percentage for visualization
        public double HealthPercentage => Math.Max(Health / MaxHealth, 0);

        // Energy percentage for visualization
        public double EnergyPercentage => Math.Max(Energy / MaxEnergy, 0);

        // Helper method to convert energy cost to health cost
        public double ConvertEnergyToHealthCost(double energyCost)
        {
            return energyCost / HealthToEnergyRatio;
        }

        // Helper method to convert health cost to energy equivalent
        public double ConvertHealthToEnergy(double healthCost)
        {
            return healthCost * HealthToEnergyRatio;
        }
    }
}
