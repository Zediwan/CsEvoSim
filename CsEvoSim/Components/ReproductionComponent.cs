using CsEvoSim.Core;

namespace CsEvoSim.Components
{
    public class ReproductionComponent : IComponent
    {
        // Reproduction cooldown
        public double ReproductionCooldown { get; set; }

        // Maximum cooldown time in seconds
        public double MaxReproductionCooldown { get; set; } = 10.0;

        // Whether the organism can reproduce right now
        public bool CanReproduce => ReproductionCooldown <= 0;

        public ReproductionComponent(double cooldown = 0.0)
        {
            ReproductionCooldown = cooldown;
        }

        // Update cooldown timer
        public void Update(double deltaTime)
        {
            if (ReproductionCooldown > 0)
            {
                ReproductionCooldown -= deltaTime;
                if (ReproductionCooldown < 0)
                    ReproductionCooldown = 0;
            }
        }

        // Reset cooldown after reproduction
        public void ResetCooldown()
        {
            ReproductionCooldown = MaxReproductionCooldown;
        }
    }
}
