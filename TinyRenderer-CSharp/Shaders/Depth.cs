using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using TinyRenderer_CSharp.Libs;

namespace TinyRenderer_CSharp.Shaders
{
    public class Depth : IShader
    {
        public Depth(bool useTex)
        {

        }

        public Rgba32 GetFragment(ref FragmentPara para)
        {
            Vector3 p = Vector3.Normalize(para.worldCoord[0] * para.baryCoord.X + para.worldCoord[1] * para.baryCoord.Y + para.worldCoord[2] * para.baryCoord.Z);
            float depth = (p.Z + 1) / 2;
            Rgba32 color = new((byte)(255 * depth), (byte)(255 * depth), (byte)(255 * depth));
            return color;
        }

        public Vector3 GetVertex(Vector3 vertex)
        {
            return vertex;
        }
    }
}