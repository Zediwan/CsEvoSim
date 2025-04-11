using CsEvoSim.Core;
using CsEvoSim.Components;
using System.Collections.Generic;

namespace CsEvoSim.Systems
{
    public class MovementSystem : ISystem
    {
        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var pos = entity.GetComponent<PositionComponent>();
                if (pos != null)
                {
                    pos.X += 1; // Just for testing
                    pos.Y += 1;
                }
            }
        }
    }
}
