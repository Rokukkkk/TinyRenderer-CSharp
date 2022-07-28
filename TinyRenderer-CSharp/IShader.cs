using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TinyRenderer_CSharp
{
    public interface IShader
    {
        Vector3 GetVertex(Vector3 vertex);
        Rgba32 GetFragment(ref Image<Rgba32> texture, ref Image<Rgba32> normal, ref Image<Rgba32> specular, ref Vector3[] screenCoord, ref Vector3[] worldCoord, ref Vector2[] uv, Vector3 baryCoord, Vector3 lightDir, Vector3 cameraPos, Vector3[] vNormal);
    }
}