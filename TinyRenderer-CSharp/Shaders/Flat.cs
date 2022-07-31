using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using TinyRenderer_CSharp.Libs;

namespace TinyRenderer_CSharp.Shaders
{
    public class Flat : IShader
    {
        readonly bool useTex;

        public Flat(bool useTex = true)
        {
            this.useTex = useTex;
        }

        public Rgba32 GetFragment(ref FragmentPara para)
        {
            // UV-interpolation (Texutre)
            Vector2 pUV = para.uv[0] * para.baryCoord.X + para.uv[1] * para.baryCoord.Y + para.uv[2] * para.baryCoord.Z;
            Vector3 pNormal = Vector3.Normalize(Vector3.Cross(para.worldCoord[1] - para.worldCoord[0], para.worldCoord[2] - para.worldCoord[0]));

            float intensity = Math.Max(0f, Vector3.Dot(pNormal, para.lightDir));

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