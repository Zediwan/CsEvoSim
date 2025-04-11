using System;
using System.Collections.Generic;
using CsEvoSim.Core;
using CsEvoSim.Utils;
using CsEvoSim.Components;

namespace CsEvoSim.Systems
{
    public class SpawnerSystem : ISystem
    {
        private readonly double _maxX;
        private readonly double _maxY;

        public int SpawnRate { get; set; } = 1;
        public double Interval { get; set; } = 1.0; // seconds
        public int MaxEntities { get; set; } = 200;
        public bool IsEnabled { get; set; } = true;

        private double _elapsed = 0;
        private readonly Random _rand = new();

        public SpawnerSystem(double maxX, double maxY)
        {
            _maxX = maxX;
            _maxY = maxY;
        }

        public void Update(List<Entity> entities)
        {
            if (!IsEnabled || entities.Count >= MaxEntities) return;

            _elapsed += 1.0 / 60.0; // assuming ~60 FPS

            if (_elapsed >= Interval)
            {
                _elapsed = 0;

                for (int i = 0; i < SpawnRate; i++)
                {
                    if (entities.Count >= MaxEntities) break;
                    var organism = OrganismFactory.CreateRandomOrganism(_maxX, _maxY);
                    entities.Add(organism);
                }
            }
        }
    }
}

