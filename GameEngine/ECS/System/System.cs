using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.ECS.System
{
    /// <summary>
    /// A system or system groups is updated after the specified system or system groups.
    /// If the specified type is not found, this attribute is ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UpdateAfterAttribute : Attribute
    {
        public Type Type { get; }
        public UpdateAfterAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// A system or system groups is updated before the specified system or system groups.
    /// If the specified type is not found, this attribute is ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UpdateBeforeAttribute : Attribute
    {
        public Type Type { get; }
        public UpdateBeforeAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// A system or group of systems must belong to the specified group.
    /// If the specified group does not exist, the group will be created automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UpdateInGroupAttribute : Attribute
    {
        public Type Type { get; }
        public UpdateInGroupAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Interface for all systems and systems groups
    /// </summary>
    public interface ISystem
    {
        void Update(float deltaTime, World world);
    }
}

