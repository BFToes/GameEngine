using System;

namespace ECS
{
    class ComponentNotFound : Exception { }
    class ComponentAlreadyExist : Exception { }
    class MaxComponentLimitExceeded : Exception { }

}
