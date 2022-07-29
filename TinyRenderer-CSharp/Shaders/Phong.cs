using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
            Vector3 pNormal = Texture.GetNormal(para.normal, pUV);
            // Diffuse(Lambertian)
            float intensity = Math.Max(0, Vector3.Dot(pNormal, para.lightDir));
            // Specular(Binn-Phong)
            Vector3 h = Vector3.Normalize((para.cameraPos - Vector3.Zero) + (para.lightDir - Vector3.Zero));
            float pSpecular = Texture.GetSpecular(para.specular, pUV);
            float sIntensity = (float)Math.Pow(Math.Max(0, Vector3.Dot(pNormal, h)), pSpecular);

            Rgba32 color = useTex ? Texture.GetColor(para.texture, pUV) : Color.White;
            return GetColor(color, 1.0f * intensity + 0.6f * sIntensity);
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
    }
}