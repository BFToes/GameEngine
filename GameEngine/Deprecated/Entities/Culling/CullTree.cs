using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Entities.Culling
{
    [Obsolete]
    class CullTree
    {
        private Node Root = new Node(Vector3.NegativeInfinity, Vector3.PositiveInfinity); // everything is inside this values dont actually matter becuase its not checked

        public CullTree(List<ICullable<ICullShape>> Entities)
        {
            foreach (ICullable<ICullShape> Entity in Entities) Root.Add(Entity);
        }
        public IEnumerable<ICullable<ICullShape>> InViewOf(IObserver<Box> Observer) => throw new NotImplementedException();
        public IEnumerable<ICullable<ICullShape>> InViewOf(IObserver<Frustum> Observer) => throw new NotImplementedException();
        public IEnumerable<ICullable<ICullShape>> InViewOf(IObserver<Sphere> Observer) => throw new NotImplementedException();

        [Obsolete]
        private class Node : Box, ICullable<Box>, ICullObserver<Box>
        {
            ICullable<ICullShape> Child1;
            ICullable<ICullShape> Child2;

            public Node(Vector3 min, Vector3 max) : base(min, max) { }
            public Node(Node Child1, Node Child2) : base(Vector3.ComponentMin(Child1.minPos, Child2.minPos), Vector3.ComponentMax(Child1.maxPos, Child2.maxPos)) { }

            Box ICullable<Box>.CullShape => this;
            Box ICullObserver<Box>.Observer => this;

            public void Add(ICullable<ICullShape> Entity)
            {
                if (Child1 is null) { Child1 = Entity; return; } 
                if (Child2 is null) { Child1 = Entity; return; }

                

            }

            public bool Detects(ICullable<Sphere> Entity) => throw new NotImplementedException();
            public bool Detects(ICullable<Box> Entity) => throw new NotImplementedException();

            public static Node operator +(Node N1, Node N2) => new Node(N1, N2);
        }
    }
}
