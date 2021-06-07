using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Graphics.SceneObjects
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
            Child.Parent = this;
        }
        public void RemoveChild(Entity Child)
        {
            Process -= Child.OnProcess;
            Child.Parent = null;
        }
        public int GetChildrenCount() => Process.GetInvocationList().Length - 1;
        public IEnumerable<Entity> GetChildren()
        {
            Delegate[] Processes = Process.GetInvocationList();
            for(int i = 1; i < Processes.Length; i++) 
                yield return (Entity)Processes[i].Target;
        }
    }

    class SpatialEntity<TransformType> : Entity where TransformType : ITransform
    {
        private Matrix4 matrix = Matrix4.Identity;
        private TransformType transform;
        public event Action<Matrix4> SetWorldMatrix = delegate { };
        public Matrix4 WorldMatrix 
        { 
            get => matrix;
            private set
            {
                matrix = value;
                SetWorldMatrix(value);
            }
        }
        
        public virtual TransformType Transform {
            get => transform;
            set { 
                foreach (Entity Child in GetChildren()) 
                {
                    if (Child is SpatialEntity<TransformType>)
                    {
                        Transform.Set_Transform -= ((SpatialEntity<TransformType>)Child).UpdateMatrix; // unsubscribe all children from old transform
                        value.Set_Transform += ((SpatialEntity<TransformType>)Child).UpdateMatrix; // subscribe all children to new transform
                    }
                        
                }
                Transform.Set_Transform -= UpdateMatrix; // unsubscribe self from old transform
                value.Set_Transform += UpdateMatrix; // subscribe self to new transform

                transform = value;
            }
        }

        public SpatialEntity(TransformType Transform)
        {
            transform = Transform;
            this.Transform.Set_Transform += UpdateMatrix;
        }
        
        public void Add(SpatialEntity<TransformType> Child)
        {
            base.AddChild(Child);
            Transform.Set_Transform += Child.UpdateMatrix;
        }
        public void Remove(SpatialEntity<TransformType> Child)
        {
            base.RemoveChild(Child);
            Transform.Set_Transform += Child.UpdateMatrix;
        }
        private void UpdateMatrix(Matrix4 _)
        {
            if (Parent != null) WorldMatrix = ((SpatialEntity<TransformType>)Parent).WorldMatrix * Transform.Matrix;
            else WorldMatrix = Transform.Matrix;
        }
    }





}
