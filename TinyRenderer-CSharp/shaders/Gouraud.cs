using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TinyRenderer_CSharp.Lib;

namespace TinyRenderer_CSharp.Shaders
{
    public class Gouraud : IShader
    {
        readonly bool useTex;

        public Gouraud(bool useTex = true)
        {
            this.useTex = useTex;
        }

        public Rgba32 GetFragment(ref Image<Rgba32> texture, ref Image<Rgba32> normal, ref Image<Rgba32> specular, ref Vector3[] screenCoord, ref Vector3[] worldCoord, ref Vector2[] uv, Vector3 baryCoord, Vector3 lightDir, Vector3 cameraPos, Vector3[] vNormal)
        {
            // UV-interpolation (Texutre)
            Vector2 pUV = uv[0] * baryCoord.X + uv[1] * baryCoord.Y + uv[2] * baryCoord.Z;
            float[] sIntensity = new float[3];

            for (int i = 0; i < 3; i++)
            {
                float pSpecular = Texture.GetSpecular(specular, pUV);
                Vector3 r = Vector3.Dot(2 * vNormal[i], lightDir) * vNormal[i] - lightDir;
                r = Vector3.Normalize(r);
                sIntensity[i] = (float)Math.Pow(Math.Max(0, r.Z), pSpecular);
            }

            float intensity1 = Vector3.Dot(vNormal[0], lightDir) + sIntensity[0];
            float intensity2 = Vector3.Dot(vNormal[1], lightDir) + sIntensity[1];
            float intensity3 = Vector3.Dot(vNormal[2], lightDir) + sIntensity[2];
            float intensity = intensity1 * baryCoord.X + intensity2 * baryCoord.Y + intensity3 * baryCoord.Z;
            intensity = Math.Max(0f, intensity);


            Rgba32 color = useTex ? Texture.GetColor(texture, pUV) : Color.White;

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