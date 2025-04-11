using System;
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

        // Digestion spectrum: -1 (herbivore) to +1 (carnivore)
        // -1: Plant digestion factor 2, Meat digestion factor 0
        //  0: Plant digestion factor 1, Meat digestion factor 1
        // +1: Plant digestion factor 0, Meat digestion factor 2
        public double DigestionSpectrum { get; set; } = 0.0;

        // Photosynthesis capability
        public bool CanPhotosynthesize { get; set; } = false;
        public double PhotosynthesisEfficiency { get; set; } = 0.0;

        // Eating/digestion state
        public double DigestCooldown { get; set; } = 0.0;
        public double MaxDigestCooldown { get; set; } = 2.0; // Seconds between feeding

        // Whether this organism is made of plant material (derived from photosynthesis ability)
        public bool IsPlantMaterial => CanPhotosynthesize;

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

        // Get plant digestion efficiency based on spectrum
        public double GetPlantDigestionEfficiency()
        {
            if (DigestionSpectrum <= -1)
                return 2.0; // Maximum plant efficiency
            else if (DigestionSpectrum >= 1)
                return 0.0; // No plant digestion
            else if (DigestionSpectrum <= 0)
                // Linear scale from -1 to 0: 2.0 down to 1.0
                return 1.0 + Math.Abs(DigestionSpectrum);
            else
                // Linear scale from 0 to 1: 1.0 down to 0.0
                return 1.0 - DigestionSpectrum;
        }

        // Get meat digestion efficiency based on spectrum
        public double GetMeatDigestionEfficiency()
        {
            if (DigestionSpectrum >= 1)
                return 2.0; // Maximum meat efficiency
            else if (DigestionSpectrum <= -1)
                return 0.0; // No meat digestion
            else if (DigestionSpectrum >= 0)
                // Linear scale from 0 to 1: 1.0 up to 2.0
                return 1.0 + DigestionSpectrum;
            else
                // Linear scale from -1 to 0: 0.0 up to 1.0
                return 1.0 + DigestionSpectrum;
        }

        // Update photosynthesis energy generation
        public void UpdatePhotosynthesis(double deltaTime, double sunlightFactor = 1.0)
        {
            if (CanPhotosynthesize && PhotosynthesisEfficiency > 0)
            {
                // Generate energy based on efficiency and available sunlight
                double energyGenerated = MaxEnergy * PhotosynthesisEfficiency * deltaTime * sunlightFactor;
                Energy = Math.Min(Energy + energyGenerated, MaxEnergy);
            }
        }

        // Update digestion cooldown
        public void UpdateDigestion(double deltaTime)
        {
            if (DigestCooldown > 0)
            {
                DigestCooldown -= deltaTime;
                if (DigestCooldown < 0)
                    DigestCooldown = 0;
            }
        }

        // Consume another organism and gain energy based on digestion efficiency
        public double Consume(EnergyComponent food)
        {
            if (DigestCooldown > 0)
                return 0.0;

            // Choose appropriate digestion efficiency based on food composition
            double efficiency = food.IsPlantMaterial ?
                GetPlantDigestionEfficiency() :
                GetMeatDigestionEfficiency();

            // Calculate raw energy available in food
            double availableEnergy = food.Energy + (food.Health * food.HealthToEnergyRatio * 0.5);

            // Calculate energy gained
            double energyGained = availableEnergy * efficiency;

            // Add the energy
            double previousEnergy = Energy;
            Energy += energyGained;

            // Cap energy at maximum
            if (Energy > MaxEnergy)
            {
                double excessEnergy = Energy - MaxEnergy;
                Energy = MaxEnergy;

                // Convert some excess energy to health
                double healthGain = ConvertEnergyToHealthCost(excessEnergy * 0.3);
                Health = Math.Min(Health + healthGain, MaxHealth);
            }

            // Start digestion cooldown
            DigestCooldown = MaxDigestCooldown;

            return energyGained;
        }
    }
}
