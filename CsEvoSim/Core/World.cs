using System.Collections.Generic;

namespace CsEvoSim.Core
{
    public class World
    {
        public List<Entity> Entities { get; } = new();
        private readonly List<ISystem> _systems = new();

        public void AddEntity(Entity entity) => Entities.Add(entity);
        public void AddSystem(ISystem system) => _systems.Add(system);

        public void Update()
        {
            foreach (var system in _systems)
                system.Update(Entities);
        }
    }
}
