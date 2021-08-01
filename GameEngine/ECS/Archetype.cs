using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.ECS
{
    /// <summary>
    /// An archetype is a unique combination of component types.
    /// <see cref="World"/> uses the archetype to group all objects that have the same sets of components.
    /// </summary>
    public class Archetype
    {
        public int ID { get; }
        public byte[] ComponentIDs;
        public readonly Archetype[] Next = new Archetype[byte.MaxValue]; // why is each archetype storing an array of max length???
        public readonly Archetype[] Prior = new Archetype[byte.MaxValue]; // shitty linked list? no indexing in a linked list
        public Entity[] Entities => _entities;
        private Entity[] _entities = new Entity[1];
        public int EntityCount { get; private set; } 

        private int _length;
        private int _count;
        private int _freeIndex = int.MaxValue;
        private readonly IComponentPool[] _compPool = new IComponentPool[byte.MaxValue];

        public Archetype(int ID, byte[] ComponentIDs)
        {
            this.ID = ID;
            this.ComponentIDs  = ComponentIDs;

            // why??? it isnt stored??? it just instantiates a pool which will be removed immediately
            foreach(byte Component in ComponentIDs)
                TypeManager.ComponentPoolCreators[Component].InstantiatePool();
        }

        public Entity this[int index]
        {
            get
            {
                if (index < 0 || index >= _length) throw new Exception("this dumbass tried to index out of range");
                RemoveHoles();
                return _entities[index];
            }
        }

        public IComponent[] GetComponents(byte index) => _compPool[index].ToArray();

        internal ComponentPool<T> GetComponentPool<T>() where T : class, IComponent, new() => (ComponentPool<T>)_compPool[ComponentType<T>.ID];
        internal void AddEntity(Entity entity)
        {
            if (_length >= _entities.Length) 
                Array.Resize(ref _entities, 2 * _entities.Length);

            entity.ArchetypeIndex = _length;
            _entities[_length++] = entity;
            _count++;
        }
        internal void RemoveEntity(Entity entity)
        {
            _entities[entity.ArchetypeIndex] = null;

            foreach (byte index in ComponentIDs)
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

                foreach (byte index in ComponentIDs)
                    _compPool[index].Replace(_freeIndex, current);

                current++;
                _freeIndex++;
            }

            _length = _freeIndex;
            _freeIndex = int.MaxValue;
        }

        internal void AddComponent(byte compIndex, IComponent component) => _compPool[compIndex].Add(_length, component);
        
    }
}
