using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    class ComponentNotFound : Exception { }
    class ComponentAlreadyExist : Exception { }
    class MaxComponentLimitExceeded : Exception { }

}
