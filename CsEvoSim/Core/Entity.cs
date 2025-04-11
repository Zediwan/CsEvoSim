using System;
using System.Collections.Generic;

namespace CsEvoSim.Core
{
    public class Entity
    {
        private readonly Dictionary<Type, IComponent> _components = new();

        public void AddComponent<T>(T component) where T : IComponent
        {
            _components[typeof(T)] = component;
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            _components.TryGetValue(typeof(T), out var comp);
            return comp as T;
        }

        public bool HasComponent<T>() where T : IComponent
        {
            return _components.ContainsKey(typeof(T));
        }
    }
}
