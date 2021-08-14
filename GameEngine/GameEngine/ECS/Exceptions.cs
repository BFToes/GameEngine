using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    class ComponentNotFoundException : Exception { }
    class ComponentFailedToAddException : Exception { }
    class ComponentFailedToRegister : Exception { }

    class ExceededSceneGraphMaxLayerException : Exception { }
    class ExceededEntityArchetypeLimitException : Exception { }
    
    
}
