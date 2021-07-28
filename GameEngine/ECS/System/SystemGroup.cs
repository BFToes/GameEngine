using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.ECS.System
{
    /// <summary>
    /// Base class for systems or other groups of systems.
    /// Using the attributes <see cref="UpdateAfterAttribute" /> and <see cref="UpdateAfterAttribute" />,
    /// the order of updating the nodes in the group is indicated.
    /// Using the attribute <see cref="UpdateInGroupAttribute" /> can be added to the parent group.
    /// </summary>
    public class SystemGroup : ISystem
    {
        public IEnumerable<ISystem> Systems => _systems;

        /// <summary>
        /// Indicates that after updating the list, nodes should be sorted.
        /// </summary>
        private bool _dirty;

        private readonly List<ISystem> _systems = new List<ISystem>();
        public void AddSystem(ISystem system)
        {
            AddSystem(GetSystemGroupHierarchy(system.GetType()), system);
        }

        /// <summary>
        /// Adds a system or group of systems to the list
        /// </summary>
        /// <param name="groupHierarchy">Parent group hierarchy</param>
        /// <param name="system">System or group of systems</param>
        private void AddSystem(Stack<Type> groupHierarchy, ISystem system)
        {
            if (groupHierarchy.Count > 0)
            {
                Type parentType = groupHierarchy.Pop();

                if (!TryGetSystemGroup(parentType, out SystemGroup systemGroup))
                {
                    systemGroup = (SystemGroup)Activator.CreateInstance(parentType);

                    _systems.Add(systemGroup);
                    _dirty = true;
                }

                systemGroup.AddSystem(groupHierarchy, system);
            }
            else
            {
                _systems.Add(system);
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets the parent group hierarchy
        /// </summary>
        /// <param name="type">Node type</param>
        /// <returns>Stack of parent nodes</returns>
        private static Stack<Type> GetSystemGroupHierarchy(Type type)
        {
            Stack<Type> groupsType = new Stack<Type>();
            UpdateInGroupAttribute attribute =
                (UpdateInGroupAttribute)Attribute.GetCustomAttribute(type, typeof(UpdateInGroupAttribute));

            while (attribute != null)
            {
                groupsType.Push(attribute.Type);
                attribute = (UpdateInGroupAttribute)Attribute.GetCustomAttribute(attribute.Type,
                    typeof(UpdateInGroupAttribute));
            }

            return groupsType;
        }

        /// <summary>
        /// Trying to find a node of the specified type
        /// </summary>
        /// <param name="type">Node type</param>
        /// <param name="systemGroup">found node</param>
        /// <returns>true - the node exists. false - node not found</returns>
        private bool TryGetSystemGroup(Type type, out SystemGroup systemGroup)
        {
            systemGroup = null;
            foreach (ISystem ecsSystem in _systems)
            {
                if (ecsSystem.GetType() != type)
                    continue;

                systemGroup = (SystemGroup)ecsSystem;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the list of systems and system groups.
        /// Before updating, sorts the items in the list, if necessary.
        /// </summary>
        /// <param name="deltaTime">Elapsed time since last update</param>
        /// <param name="world">Entity Manager <see cref="World"/></param>
        public void Update(float deltaTime, World world)
        {
            if (_dirty)
            {
                SystemSorter.Sort(_systems);
                _dirty = false;
            }

            foreach (ISystem system in _systems)
            {
                system.Update(deltaTime, world);
            }
        }
    }
}
