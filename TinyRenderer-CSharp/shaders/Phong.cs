using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TinyRenderer_CSharp.Lib;

namespace TinyRenderer_CSharp.Shaders
{
    public class Phong : IShader
    {
        readonly bool useTex;

        public Phong(bool useTex = true)
        {
            this.useTex = useTex;
        }

        public Rgba32 GetFragment(ref Image<Rgba32> texture, ref Vector3[] screenCoord, ref Vector2[] uv, Vector3 baryCoord, ref Vector3[] normal, Vector3 lightDir)
        {
            // UV-interpolation (Texutre)
            Vector2 pUV = uv[0] * baryCoord.X + uv[1] * baryCoord.Y + uv[2] * baryCoord.Z;

            Vector3 pNormal = normal[0] * baryCoord.X + normal[1] * baryCoord.Y + normal[2] * baryCoord.Z;
            float intensity = Vector3.Dot(pNormal, lightDir);
            if (intensity < -0.001) intensity = 0;
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