using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TinyRenderer_CSharp.Libs
{
    public struct FragmentPara
    {
        public Image<Rgba32> texture;
        public Image<Rgba32> normal;
        public Image<Rgba32> specular;
        public Vector3[] screenCoord;
        public Vector3[] worldCoord;
        public Vector2[] uv;
        public Vector3 baryCoord;
        public Vector3 lightDir;
        public Vector3 cameraPos;
        public Vector3[] vNormal;
        public float zInterpolation;

        public FragmentPara(ref Image<Rgba32> texture, ref Image<Rgba32> normal, ref Image<Rgba32> specular, ref Vector3[] screenCoord, ref Vector3[] worldCoord, ref Vector2[] uv, ref Vector3 baryCoord, ref Vector3 lightDir, ref Vector3 cameraPos, ref Vector3[] vNormal, ref float zInterpolation)
        {
            this.texture = texture;
            this.normal = normal;
            this.specular = specular;
            this.screenCoord = screenCoord;
            this.worldCoord = worldCoord;
            this.uv = uv;
            this.baryCoord = baryCoord;
            this.lightDir = lightDir;
            this.cameraPos = cameraPos;
            this.vNormal = vNormal;
            this.zInterpolation = zInterpolation;
        }
    }
}