using ECS;

// TODO: Hierarchy Component

namespace Engine.Components
{
    struct Hierarchy : IComponent
    {
        public bool dirtyFlag;

        
    } 

    public class UpdateHierachy : Behaviour
    {

        public UpdateHierachy() : base(new byte[]{ ComponentManager.ID<Hierarchy>() }, new byte[]{ }, new byte[]{ })
        {
            
        }
    }


}