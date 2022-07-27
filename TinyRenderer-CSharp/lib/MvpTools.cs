using System.Numerics;

namespace TinyRenderer_CSharp.Lib
{
    static class MvpTools
    {
        static Matrix4x4 ModelViewMatrix;
        static Matrix4x4 ProjectionMartix;
        static Matrix4x4 ViewportMartix;

        public static void SetMvpTools(Camera camera, int width, int height, int depth)
        {
            ModelViewMatrix = GetModelViewMatrix(camera);
            ProjectionMartix = GetProjectionMatrix(camera);
            ViewportMartix = GetViewportMatrix(depth, width / 8, height / 8, width * 3 / 4, height * 3 / 4);
        }

        public static Vector3 GetScreenCoord(Vector3 worldCoord)
        {
            Vector3 screenCoord = Homo2Vertices(ViewportMartix * GetProjectionDivision(ProjectionMartix * ModelViewMatrix * Local2Homo(worldCoord)));

            return screenCoord;
        }

        static Matrix4x4 GetProjectionDivision(Matrix4x4 m)
        {
            m.M11 /= m.M41;
            m.M21 /= m.M41;
            m.M31 /= m.M41;
            m.M41 = 1.0f;

            return m;
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

        static Matrix4x4 GetProjectionMatrix(Camera camera)
        {
            Matrix4x4 projection = Matrix4x4.Identity;
            projection.M43 = -1 / camera.Position.Z;

            return projection;
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
    }
}