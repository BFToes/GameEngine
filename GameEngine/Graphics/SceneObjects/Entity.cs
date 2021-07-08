using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Graphics.Entities
{
    class Entity
    {
        private event Action<float> Process = delegate { };
        public Entity Parent { get; private set; }
        public virtual void OnProcess(float delta)
        {
            Process(delta);
        }
        public void AddChild(Entity Child)
        {
            Process += Child.OnProcess;
            Child.OnAdded(this);
        }
        public void RemoveChild(Entity Child)
        {
            Process -= Child.OnProcess;
            Child.OnRemoved(this);
        }
        protected virtual void OnAdded(Entity Parent) => this.Parent = Parent;
        protected virtual void OnRemoved(Entity Parent) => this.Parent = null;


        public int GetChildrenCount() => Process.GetInvocationList().Length - 1;
        public IEnumerable<Entity> GetChildren()
        {
            Delegate[] Processes = Process.GetInvocationList();
            for(int i = 1; i < Processes.Length; i++) 
                yield return (Entity)Processes[i].Target;
        }
    }

    interface ISpatial
    {
        public event Action<Matrix4> Set_WorldMatrix;
        public Matrix4 WorldMatrix { get; }
    }

    class SpatialEntity<TransformType> : Entity, ISpatial where TransformType : ITransform
    {
        private Matrix4 BaseMatrix = Matrix4.Identity; 
        public event Action<Matrix4> Set_WorldMatrix = delegate { };
        public Matrix4 WorldMatrix 
        { 
            get => BaseMatrix; 
            private set 
            { 
                BaseMatrix = value; 
                Set_WorldMatrix(value); 
            } 
        }
        public Vector3 WorldPosition 
        { 
            get => new Vector3(WorldMatrix.Row3); 
        }
        public TransformType Transform 
        { 
            get; 
            private set; 
        }

        public SpatialEntity(TransformType Transform)
        {
            this.Transform = Transform;
            this.Transform.Set_Transform += UpdateMatrix;
        }
        
        public virtual void Add<T>(SpatialEntity<T> Child) where T : ITransform
        {
            base.AddChild(Child);
            Set_WorldMatrix += Child.UpdateMatrix;
            Child.WorldMatrix = Child.Transform.Matrix * this.WorldMatrix;
        }
        public virtual void Remove<T>(SpatialEntity<T> Child) where T : ITransform
        {
            base.RemoveChild(Child);
            Set_WorldMatrix += Child.UpdateMatrix;
            Child.Transform.Extract(WorldMatrix);
        }
        private void UpdateMatrix(Matrix4 _)
        {
            if (Parent != null)  WorldMatrix = Transform.Matrix * ((ISpatial)Parent).WorldMatrix;
            else WorldMatrix = Transform.Matrix;
        }
    }
}
