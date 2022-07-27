using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TinyRenderer_CSharp
{
    public interface IShader
    {
        Vector3 GetVertex(Vector3 vertex);
        Rgba32 GetFragment(ref Image<Rgba32> texture, ref Vector3[] screenCoord, ref Vector2[] uv, Vector3 baryCoord, ref Vector3[] normal, Vector3 lightDir);
    }
}