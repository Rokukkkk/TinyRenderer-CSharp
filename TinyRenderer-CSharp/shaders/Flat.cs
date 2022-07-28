using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TinyRenderer_CSharp.Lib;

namespace TinyRenderer_CSharp.Shaders
{
    public class Flat : IShader
    {
        readonly bool useTex;

        public Flat(bool useTex = true)
        {
            this.useTex = useTex;
        }

        public Rgba32 GetFragment(ref Image<Rgba32> texture, ref Image<Rgba32> normal, ref Image<Rgba32> specular, ref Vector3[] screenCoord, ref Vector3[] worldCoord, ref Vector2[] uv, Vector3 baryCoord, Vector3 lightDir, Vector3 cameraPos, Vector3[] vNormal)
        {
            // UV-interpolation (Texutre)
            Vector2 pUV = uv[0] * baryCoord.X + uv[1] * baryCoord.Y + uv[2] * baryCoord.Z;

            Vector3 pNormal = Vector3.Cross(worldCoord[1] - worldCoord[0], worldCoord[2] - worldCoord[0]);
            pNormal = Vector3.Normalize(pNormal);

            float intensity = Math.Max(0f, Vector3.Dot(pNormal, lightDir));
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