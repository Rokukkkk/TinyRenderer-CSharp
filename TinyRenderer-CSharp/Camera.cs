using System.Numerics;

namespace TinyRenderer_CSharp
{
    public struct Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 WorldUp = Vector3.UnitY;
        public Vector3 Front = -Vector3.UnitZ;
        public Vector3 Right;
        public Vector3 Up;

        public Camera(Vector3 position, Vector3 worldUp, Vector3 front)
        {
            Position = position;
            WorldUp = worldUp;
            Front = Vector3.Normalize(front);
            Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}

