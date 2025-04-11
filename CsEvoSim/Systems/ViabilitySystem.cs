using System.Collections.Generic;
using CsEvoSim.Components;
using CsEvoSim.Core;

namespace CsEvoSim.Systems
{
    public class ViabilitySystem : ISystem
    {
        public void Update(List<Entity> entities)
        {
            List<Entity> nonViableEntities = new List<Entity>();

            // Check each organism for viability
            foreach (var entity in entities)
            {
                var dna = entity.GetComponent<DNAComponent>();

                if (dna == null || !dna.IsViable)
                {
                    nonViableEntities.Add(entity);
                }
            }

            // Remove all non-viable entities
            foreach (var entity in nonViableEntities)
            {
                entities.Remove(entity);
            }
        }
    }
}