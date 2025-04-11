using System;
using System.Collections.Generic;
using CsEvoSim.Core;
using CsEvoSim.Utils;
using CsEvoSim.Components;

namespace CsEvoSim.Systems
{
    public class SpawnerSystem : ISystemWithSettings
    {
        private readonly double _maxX;
        private readonly double _maxY;

        public int SpawnRate { get; set; } = 1;
        public double Interval { get; set; } = 1.0; // seconds
        public int MaxEntities { get; set; } = 200;
        public bool IsEnabled { get; set; } = true;

        private double _elapsed = 0;
        private readonly Random _rand = new();

        public string SettingsGroupName => "Spawner";

        public SpawnerSystem(double maxX, double maxY)
        {
            _maxX = maxX;
            _maxY = maxY;
        }

        public IEnumerable<SystemSetting> GetSettings()
        {
            yield return SystemSetting.CreateBoolean(
                "IsEnabled",
                "Enable Spawning",
                IsEnabled,
                val => IsEnabled = val,
                "Controls whether new organisms will spawn"
            );

            yield return SystemSetting.CreateNumeric(
                "SpawnRate",
                "Spawn Rate",
                SpawnRate, 1, 20, 1,
                val => SpawnRate = val,
                "Number of new organisms to spawn each interval"
            );

            yield return SystemSetting.CreateNumeric(
                "Interval",
                "Spawn Interval (s)",
                Interval, 0.1, 10.0, 0.1,
                val => Interval = val,
                "Time between spawning new organisms"
            );

            yield return SystemSetting.CreateNumeric(
                "MaxEntities",
                "Maximum Organisms",
                MaxEntities, 50, 1000, 10,
                val => MaxEntities = val,
                "Maximum number of organisms allowed in the simulation"
            );
        }

        public void Update(List<Entity> entities)
        {
            // Existing implementation
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
