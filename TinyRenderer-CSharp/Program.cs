using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Numerics;

namespace TinyRenderer_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            int width = 800;
            int height = 800;

            Image<Rgb24> image = new(width + 1, height + 1);  // Gird 1 to 800 only got 799 pixels

            var model = Model.LoadModel(@"./obj/african_head.obj");

            // Render the mesh lines
            foreach (var item in model)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 v0 = item[i].Vertex;
                    Vector3 v1 = item[(i + 1) % 3].Vertex;
                    int x0 = (int)((v0.X + 1) * width / 2);
                    int y0 = (int)((v0.Y + 1) * height / 2);
                    int x1 = (int)((v1.X + 1) * width / 2);
                    int y1 = (int)((v1.Y + 1) * height / 2);

                    DrawLine(x0, y0, x1, y1, ref image, Color.Cyan);
                }
            }

            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));  //Flip the image upside down
            SaveFrame(ref image);
        }


        // Bresenham’s Line Drawing Algorithm
        static void DrawLine(int x0, int y0, int x1, int y1, ref Image<Rgb24> image, Rgb24 color)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int yStep = (y0 < y1) ? 1 : -1;
            int y = y0;

            for (int x = x0; x <= x1; x++)
            {
                if (steep) image[y, x] = color;
                else image[x, y] = color;

                error -= dy;
                if (error < 0)
                {
                    y += yStep;
                    error += dx;
                }
            }
        }

        // Save frame to tga file
        static void SaveFrame(ref Image<Rgb24> image)
        {
            image.SaveAsTga("./out.tga");
        }

        // Swap two number
        static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }
    }
}