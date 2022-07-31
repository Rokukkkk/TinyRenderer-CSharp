using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using TinyRenderer_CSharp.Libs;

namespace TinyRenderer_CSharp.Shaders
{
    public class Phong : IShader
    {
        readonly bool useTex;

        public Phong(bool useTex = true)
        {
            this.useTex = useTex;
        }

        public Rgba32 GetFragment(ref FragmentPara para)
        {
            // UV-interpolation (Texutre)
            Vector2 pUV = para.uv[0] * para.baryCoord.X + para.uv[1] * para.baryCoord.Y + para.uv[2] * para.baryCoord.Z;
            Vector3 pNormal = Tangent2Obj(ref para);

            // Diffuse(Lambertian)
            float intensity = Math.Max(0, Vector3.Dot(pNormal, para.lightDir));
            // Specular(Binn-Phong)
            Vector3 h = Vector3.Normalize((para.cameraPos - Vector3.Zero) + (para.lightDir - Vector3.Zero));
            float pSpecular = Texture.GetSpecular(para.specular, pUV);
            float sIntensity = (float)Math.Pow(Math.Max(0, Vector3.Dot(pNormal, h)), pSpecular);

            Rgba32 color = useTex ? Texture.GetColor(para.texture, pUV) : Color.White;
            return GetColor(color, (intensity + 0.6f * sIntensity) * para.shadow);
        }

        public Vector3 GetVertex(Vector3 vertex)
        {
            return vertex;
        }

        static Rgba32 GetColor(Rgba32 color, float intensity)
        {
            color.R = (byte)(color.R * intensity);
            color.G = (byte)(color.G * intensity);
            color.B = (byte)(color.B * intensity);

            return color;
        }

        // Method for using Tangent Normal Map
        static Vector3 Tangent2Obj(ref FragmentPara para)
        {
            Vector2 pUV = para.uv[0] * para.baryCoord.X + para.uv[1] * para.baryCoord.Y + para.uv[2] * para.baryCoord.Z;
            Vector3 tNormal = Texture.GetNormal(para.normal, pUV);
            Vector3 pNormal = para.vNormal[0] * para.baryCoord.X + para.vNormal[1] * para.baryCoord.Y + para.vNormal[2] * para.baryCoord.Z;

            Matrix4x4 A = Matrix4x4.Identity;
            A.M11 = para.worldCoord[1].X - para.worldCoord[0].X; A.M12 = para.worldCoord[1].Y - para.worldCoord[0].Y; A.M13 = para.worldCoord[1].Z - para.worldCoord[0].Z;
            A.M21 = para.worldCoord[2].X - para.worldCoord[0].X; A.M22 = para.worldCoord[2].Y - para.worldCoord[0].Y; A.M23 = para.worldCoord[2].Z - para.worldCoord[0].Z;
            A.M31 = pNormal.X; A.M32 = pNormal.Y; A.M33 = pNormal.Z;

            if (Matrix4x4.Invert(A, out Matrix4x4 AI)) A = AI;
            Matrix4x4 iM = new(); Matrix4x4 jM = new();
            iM.M11 = para.uv[1].X - para.uv[0].X; iM.M21 = para.uv[2].X - para.uv[0].X;
            jM.M11 = para.uv[1].Y - para.uv[0].Y; jM.M21 = para.uv[2].Y - para.uv[0].Y;
            iM = A * iM; jM = A * jM;
            Vector3 i = Vector3.Normalize(new(iM.M11, iM.M21, iM.M31)); Vector3 j = Vector3.Normalize(new(jM.M11, jM.M21, jM.M31));

            Matrix4x4 B = new();
            B.M11 = i.X; B.M21 = i.Y; B.M31 = i.Z;
            B.M12 = j.X; B.M22 = j.Y; B.M32 = j.Z;
            B.M13 = pNormal.X; B.M23 = pNormal.Y; B.M33 = pNormal.Z;
            Matrix4x4 normalM = new(); normalM.M11 = tNormal.X; normalM.M21 = tNormal.Y; normalM.M31 = tNormal.Z;
            B *= normalM;

            return Vector3.Normalize(new(B.M11, B.M21, B.M31));
        }
    }
}