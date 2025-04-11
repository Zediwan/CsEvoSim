using System.Collections.Generic;

namespace CsEvoSim.Core
{
    public interface ISystem
    {
        void Update(List<Entity> entities);
    }
}
