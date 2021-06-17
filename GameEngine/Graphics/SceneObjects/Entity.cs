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

    interface Spatial
    {
        public event Action<Matrix4> Set_WorldMatrix;
        public Matrix4 WorldMatrix { get; }
    }

    class SpatialEntity<TransformType> : Entity, Spatial where TransformType : ITransform
    {
        private Matrix4 matrix = Matrix4.Identity;
        private TransformType transform;
        public event Action<Matrix4> Set_WorldMatrix = delegate { };
        public Matrix4 WorldMatrix 
        { 
            get => matrix;
            private set
            {
                matrix = value;
                Set_WorldMatrix(value);
            }
        }
        public Vector3 WorldPosition => new Vector3(WorldMatrix.Row3);
        
        public virtual TransformType Transform {
            get => transform;
        }

        public SpatialEntity(TransformType Transform)
        {
            transform = Transform;
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
        }
        private void UpdateMatrix(Matrix4 _)
        {
            if (Parent != null)  WorldMatrix = Transform.Matrix * ((Spatial)Parent).WorldMatrix;
            else WorldMatrix = Transform.Matrix;
        }
    }





}
