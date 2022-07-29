using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
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
            Rgba32 color = new((byte)para.zInterpolation, (byte)para.zInterpolation, (byte)para.zInterpolation);
            return color;
        }

        public Vector3 GetVertex(Vector3 vertex)
        {
            return vertex;
        }
    }
}