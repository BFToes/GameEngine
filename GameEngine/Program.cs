using System;
using ECS;


namespace GameEngine
{
    struct C1 : IComponent { public string data; }
    struct C2 : IComponent { public string data; }
    struct C3 : IComponent { public string data; }
    struct C4 : IComponent { public string data; }


    class thing : Entity
    {
        public ref C1 c1 => ref GetComponent<C1>();
        public ref C2 c2 => ref GetComponent<C2>();
        public ref C3 c3 => ref GetComponent <C3>();
        public ref C4 c4 => ref GetComponent <C4>();


        public thing() : base(ComponentManager.ID<C1, C2, C3, C4>()) 
        {
            c1.data = "things have changed";
        }
    }


    class Program
    {
        
        static public void Main(string[] args)
        {
            thing t = new thing();
        }
    }
}