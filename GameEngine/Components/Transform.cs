using System;
using System.Collections.Generic;
using System.Text;

using ECS;
using OpenTK.Mathematics;

namespace GameEngine.Components
{
    struct Transform : IComponent
    {
        public bool dirtyFlag;
        public Matrix4 matrix;

        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;

        public Vector3 WorldPosition => new Vector3(matrix.Column3);
        public Vector3 Position 
        { 
            get => _position;
            set
            {
                dirtyFlag = true;
                _position = value;
            }
        }
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                dirtyFlag = true;
                _scale = value;
            }
        }
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                dirtyFlag = true;
                _scale = value;
            }
        }

        public static Transform Default => new Transform();

        public Transform(Vector3 Position, Vector3 Rotation, Vector3 Scale)
        {
            dirtyFlag = true;
            _position = Position;
            _rotation = Rotation;
            _scale = Scale;
            matrix = Matrix4.Identity;
        }

        public static Matrix4 CalcMatrix(Vector3 Position, Vector3 Rotation, Vector3 Scale)
        {
            Matrix4 pmat = Matrix4.CreateTranslation(Position);
            Matrix4 rmat = Matrix4.CreateTranslation(Rotation);
            Matrix4 smat = Matrix4.CreateTranslation(Scale);
            
            return smat * rmat * pmat;
        }

    }
}
