using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.ECS
{
    public interface IArchetype
    {
        int ID { get; } // unique ID/Index of Archetype
        byte[] Indices { get; } // the Indices of the components in the archetype
        int EntityCount { get; } // the number of entities that use this archetype
        IEntity this[int index] { get; } // an indexer of the entities that use this archetype
        internal IComponentPool GetComponentPool(byte index);
    }
    /// <summary>
    /// 
    /// </summary>
    public class Archetype : IArchetype
    {
        public int ID { get; }
        public byte[] Indices { get; }
        public int EntityCount
        {
            get
            {
                RemoveHoles();
                return _length;
            }
        }
        public readonly HashSet<byte> SetIndices;
        public readonly Archetype[] Next = new Archetype[byte.MaxValue]; // why is each archetype storing an array
        public readonly Archetype[] Prior = new Archetype[byte.MaxValue];
        
        private int _length;
        private int _count;
        private int _freeIndex = int.MaxValue;
        private Entity[] _entities = new Entity[1];
        private readonly IComponentPool[] _compPool = new IComponentPool[byte.MaxValue];

        public Archetype(int ID, byte[] Indices)
        {
            this.ID = ID;
            this.Indices = Indices;
            SetIndices = new HashSet<byte>(Indices);

            foreach(byte index in Indices)
                TypeManager.ComponentPoolCreators[index].InstantiatePool();
        }

        public IEntity this[int index]
        {
            get
            {
                if (index < 0 || index >= _length) throw new Exception("Dumbass");
                RemoveHoles();
                return _entities[index];
            }
        }
        internal ComponentPool<TC> GetComponentPool<TC>() where TC : class, IComponent, new() => (ComponentPool<TC>)_compPool[ComponentType<TC>.Index];
        IComponentPool IArchetype.GetComponentPool(byte index) => _compPool[index];
        public Entity[] GetEntities(out int length)
        {
            RemoveHoles();
            length = _length;
            return _entities;
        }
        internal void AddEntity(Entity entity)
        {
            if (_length >= _entities.Length) Array.Resize(ref _entities, 2 * _entities.Length);

            entity.ArchetypeIndex = _length;
            _entities[_length++] = entity;
            _count++;
        }
        internal void AddComponent(byte compIndex, IComponent component) => _compPool[compIndex].Add(_length, component);
        internal void RemoveEntity(Entity entity)
        {
            _entities[entity.ArchetypeIndex] = null;

            foreach (byte index in Indices)
                _compPool[index].Remove(entity.ArchetypeIndex);

            _freeIndex = Math.Min(_freeIndex, entity.ArchetypeIndex);
            _count--;

            if (_freeIndex == _length - 1)
            {
                _length = _freeIndex;
                _freeIndex = int.MaxValue;
            }
            else if (_length >= _count + _count)  RemoveHoles();
        }

        private void RemoveHoles()
        {
            if (_freeIndex >= _length) return;

            int current = _freeIndex + 1;
            while (current < _length)
            {
                while (current < _length && _entities[current] == null) current++;

                if (current >= _length) continue;

                Entity entity = _entities[current];
                entity.ArchetypeIndex = _freeIndex;

                _entities[_freeIndex] = entity;
                _entities[current] = null;

                foreach (byte index in Indices)
                    _compPool[index].Replace(_freeIndex, current);

                current++;
                _freeIndex++;
            }

            _length = _freeIndex;
            _freeIndex = int.MaxValue;
        }
    }
}
