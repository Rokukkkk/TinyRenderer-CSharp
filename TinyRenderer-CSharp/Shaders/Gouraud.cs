using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using TinyRenderer_CSharp.Libs;

namespace TinyRenderer_CSharp.Shaders
{
    public class Gouraud : IShader
    {
        readonly bool useTex;

        public Gouraud(bool useTex = true)
        {
            this.useTex = useTex;
        }

        public Rgba32 GetFragment(ref FragmentPara para)
        {
            // UV-interpolation (Texutre)
            Vector2 pUV = para.uv[0] * para.baryCoord.X + para.uv[1] * para.baryCoord.Y + para.uv[2] * para.baryCoord.Z;

            Vector3 h = Vector3.Normalize((para.cameraPos - Vector3.Zero) + (para.lightDir - Vector3.Zero));
            float pSpecular = Texture.GetSpecular(para.specular, pUV);
            float[] sIntensity = new float[3];
            for (int i = 0; i < 3; i++)
            {
                sIntensity[i] = (float)Math.Pow(Math.Max(0, Vector3.Dot(para.vNormal[i], h)), pSpecular);
            }

            float intensity1 = Vector3.Dot(para.vNormal[0], para.lightDir) + sIntensity[0];
            float intensity2 = Vector3.Dot(para.vNormal[1], para.lightDir) + sIntensity[1];
            float intensity3 = Vector3.Dot(para.vNormal[2], para.lightDir) + sIntensity[2];
            float intensity = intensity1 * para.baryCoord.X + intensity2 * para.baryCoord.Y + intensity3 * para.baryCoord.Z;
            intensity = Math.Max(0f, intensity);

            Rgba32 color = useTex ? Texture.GetColor(para.texture, pUV) : Color.White;
            return GetColor(color, intensity);
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