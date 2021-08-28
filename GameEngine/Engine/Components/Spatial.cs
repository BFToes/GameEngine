using ECS;
using OpenTK.Mathematics;

namespace Engine.Components
{
    public struct TransformComponent : IComponent
    {
        private bool _dirtyFlag; 
        
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;

        public Matrix4 Matrix { get; private set; }
        public Vector3 Position 
        {
            get => _position;
            set
            {
                _dirtyFlag = true;
                _position = value;
            }
        }
        public Quaternion Rotation 
        {
            get => _rotation;
            set
            {
                _dirtyFlag = true;
                _rotation = value;
            }
        }
        public Vector3 Scale 
        {
            get => _scale;
            set
            {
                _dirtyFlag = true;
                _scale = value;
            }
        }

        public TransformComponent(Vector3 Scale, Quaternion Rotation, Vector3 Position)
        {
            _dirtyFlag = false;
            _scale = Scale;
            _rotation = Rotation;
            _position = Position;
            Matrix = Matrix4.Identity;
        }

        private static Matrix4 CalculateTransform(Vector3 Position, Quaternion Rotation, Vector3 Scale)
        {
            Matrix4 RotationMatrix = Matrix4.CreateFromQuaternion(Rotation);
            Matrix4 ScaleMatrix = Matrix4.CreateScale(Scale);
            Matrix4 TranslationMatrix = Matrix4.CreateTranslation(Position);
            return RotationMatrix * ScaleMatrix * TranslationMatrix;
        }
        private static Matrix4 CalculateTransform(Matrix4 Base, Vector3 Position, Quaternion Rotation, Vector3 Scale)
        {
            Matrix4 RotationMatrix = Matrix4.CreateFromQuaternion(Rotation);
            Matrix4 ScaleMatrix = Matrix4.CreateScale(Scale);
            Matrix4 TranslationMatrix = Matrix4.CreateTranslation(Position);
            return RotationMatrix * ScaleMatrix * TranslationMatrix * Base;
        }       
    }
}