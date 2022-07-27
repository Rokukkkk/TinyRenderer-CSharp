using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Numerics;

namespace TinyRenderer_CSharp.Lib
{
    public static class Texture
    {
        public static Image<Rgba32> LoadTexture(string path)
        {
            var image = Image.Load(path);
            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            return (Image<Rgba32>)image;
        }


        public static Rgba32 GetColor(this Image<Rgba32> image, Vector2 uv) =>
            image[(int)(uv.X * image.Width), (int)(uv.Y * image.Height)];


        public static Vector3 GetNormal(this Image<Rgba32> image, Vector2 uv)
        {
            var color = image[(int)(uv.X * image.Width), (int)(uv.Y * image.Height)];

            return Vector3.Normalize(new Vector3(color.R / 225f * 2 - 1, color.G / 225f * 2 - 1, color.B / 225f * 2 - 1));
        }


        public static float GetSpecular(this Image<Rgba32> image, Vector2 uv) =>
            image[(int)(uv.X * image.Width), (int)(uv.Y * image.Height)].R;


        public static void SetColor(this Image<Rgba32> image, Vector2 uv, Rgba32 color) =>
            image[(int)(uv.X * image.Width), (int)(uv.Y * image.Height)] = color;
    }
}

