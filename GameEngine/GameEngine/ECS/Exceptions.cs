using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    class ComponentNotFound : Exception { }
    class ComponentAlreadyExists : Exception { }
    class DuplicateEntityException : Exception { }
    class ComponentPoolException : Exception { }
    class EntityNotFoundException : Exception { }
    class IndependentEntityException : Exception { }
    class NoCompatibleEntitiesException : Exception { }
    class NullEntityPoolException : Exception { }
    class EntityCachedException : Exception  { }
}
