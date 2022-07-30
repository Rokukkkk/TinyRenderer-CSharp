using System.Numerics;

namespace TinyRenderer_CSharp.Libs
{
    public class Geometric
    {
        Matrix4x4 ModelViewMatrix;
        Matrix4x4 ProjectionMartix;
        Matrix4x4 ViewportMartix;

        public Geometric(Camera camera, int width, int height, int depth)
        {
            ModelViewMatrix = GetModelViewMatrix(camera);
            ProjectionMartix = GetProjectionMatrix(camera);
            ViewportMartix = GetViewportMatrix(depth, width / 8, height / 8, width * 3 / 4, height * 3 / 4);
        }

        public Vector3 GetScreenCoord(Vector3 worldCoord)
        {
            return Homo2Vertices(ViewportMartix * GetProjectionDivision(ProjectionMartix * ModelViewMatrix * Local2Homo(worldCoord)));
        }

        public Matrix4x4 GetMvp()
        {
            return ViewportMartix * ProjectionMartix * ModelViewMatrix;
        }

        static Matrix4x4 GetProjectionDivision(Matrix4x4 m)
        {
            m.M11 /= m.M41;
            m.M21 /= m.M41;
            m.M31 /= m.M41;
            m.M41 = 1.0f;

            return m;
        }

        static Matrix4x4 GetModelViewMatrix(Camera camera)
        {
            Matrix4x4 r = Matrix4x4.Identity;
            Matrix4x4 t = Matrix4x4.Identity;

            r.M11 = camera.Right.X;
            r.M21 = camera.Up.X;
            r.M31 = -camera.Front.X;
            t.M14 = -camera.Position.X;
            r.M12 = camera.Right.Y;
            r.M22 = camera.Up.Y;
            r.M32 = -camera.Front.Y;
            t.M24 = -camera.Position.Y;
            r.M13 = camera.Right.Z;
            r.M23 = camera.Up.Z;
            r.M33 = -camera.Front.Z;
            t.M34 = -camera.Position.Z;

            return r * t;
        }

        static Matrix4x4 GetViewportMatrix(int depth, int x, int y, int w, int h)
        {
            Matrix4x4 m = Matrix4x4.Identity;

            m.M14 = x + w / 2.0f;
            m.M24 = y + h / 2.0f;
            m.M34 = depth / 2.0f;

            m.M11 = w / 2.0f;
            m.M22 = h / 2.0f;
            m.M33 = depth / 2.0f;

            return m;
        }

        static Matrix4x4 GetProjectionMatrix(Camera camera)
        {
            Matrix4x4 projection = Matrix4x4.Identity;
            projection.M43 = -1f / Vector3.Distance(Vector3.Zero, camera.Position);
            projection.M44 = 0.1f;

            return projection;
        }

        static Matrix4x4 Local2Homo(Vector3 local)
        {
            Matrix4x4 m = new();
            m.M11 = local.X;
            m.M21 = local.Y;
            m.M31 = local.Z;
            m.M41 = 1.0f;

            return m;
        }

        static Vector3 Homo2Vertices(Matrix4x4 m)
        {
            return new Vector3(m.M11, m.M21, m.M31);
        }

        public static Vector3 GetBarycentric(Vector3[] screenCoord, Vector3 p)
        {
            Vector3 v0 = screenCoord[1] - screenCoord[0];
            Vector3 v1 = screenCoord[2] - screenCoord[0];
            Vector3 v2 = p - screenCoord[0];
            float den = v0.X * v1.Y - v1.X * v0.Y;
            float v = (v2.X * v1.Y - v1.X * v2.Y) / den;
            float w = (v0.X * v2.Y - v2.X * v0.Y) / den;
            float u = 1.0f - v - w;

            return new Vector3(u, v, w);
        }
    }
}