using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace TinyRenderer_CSharp.Libs
{
    public interface IShader
    {
        Vector3 GetVertex(Vector3 vertex);
        Rgba32 GetFragment(ref FragmentPara para);
    }
}