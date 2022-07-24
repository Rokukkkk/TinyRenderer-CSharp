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
            // Initiate frame
            int width = 2000;
            int height = 2000;
            Image<Rgba32> image = new(width + 1, height + 1);  // Gird 1 to 800 only got 799 pixels

            // Load the model
            var model = Model.LoadModel(@"./obj/african_head.obj");

            Vector3 lightDir = new(0, 0, -1);
            Vector2[] screenCoord = new Vector2[3];
            Vector3[] worldCoord = new Vector3[3];

            foreach (var item in model)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 v = item[i].Vertex;
                    screenCoord[i] = new Vector2((v.X + 1) * width / 2, (v.Y + 1) * height / 2);
                    worldCoord[i] = v;
                }

                Vector3 normal = Vector3.Cross(worldCoord[2] - worldCoord[0], worldCoord[1] - worldCoord[0]);
                normal = Vector3.Normalize(normal);
                float intensity = Vector3.Dot(normal, lightDir);

                if (intensity > 0)
                {
                    byte rgb = (byte)(intensity * 255);
                    DrawTriangle(screenCoord, ref image, new Rgba32(rgb, rgb, rgb, 255));
                }

                /*
                // Draw all the triangles with random color
                byte r = (byte)(new Random()).Next(255);  // Rgb value should be byte type
                byte g = (byte)(new Random()).Next(255);
                byte b = (byte)(new Random()).Next(255);
                DrawTriangle(screenCoord, ref image, new Rgba32(r, g, b, 255));
                */
            }


            /*
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

                    DrawLine(x0, y0, x1, y1, ref image, Color.White);
                }
            }
            */


            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));  //Flip the image upside down

            SaveFrame(ref image);

        }

        static void DrawTriangle(Vector2[] pts, ref Image<Rgba32> image, Rgba32 color)
        {
            Vector2 bboxMin = new(image.Width - 1, image.Height - 1);
            Vector2 bboxMax = new(0, 0);

            bboxMin.X = (new float[] { bboxMin.X, pts[0].X, pts[1].X, pts[2].X }).Min();
            bboxMin.Y = (new float[] { bboxMin.Y, pts[0].Y, pts[1].Y, pts[2].Y }).Min();
            bboxMax.X = (new float[] { bboxMax.X, pts[0].X, pts[1].X, pts[2].X }).Max();
            bboxMax.Y = (new float[] { bboxMax.Y, pts[0].Y, pts[1].Y, pts[2].Y }).Max();

            for (int i = (int)bboxMin.X; i < bboxMax.X; i++)
            {
                for (int j = (int)bboxMin.Y; j < bboxMax.Y; j++)
                {
                    Vector2 p = new(i, j);
                    Vector3 baryCoord = GetBarycentric(pts, p);
                    if (baryCoord.X < 0 || baryCoord.Y < 0 || baryCoord.Z < 0) { continue; }
                    image[(int)p.X, (int)p.Y] = color;
                }
            }

            /*
            if (t0.Y == t1.Y && t0.Y == t2.Y) { return; }
            // Sort 3 vertices based on Y
            if (t0.Y > t1.Y) { Swap(ref t0, ref t1); }
            if (t0.Y > t2.Y) { Swap(ref t0, ref t2); }
            if (t1.Y > t2.Y) { Swap(ref t1, ref t2); }

            int totalHeight = (int)(t2.Y - t0.Y);

            for (int i = 0; i < totalHeight; i++)
            {
                bool secondHalf = i > t1.Y - t0.Y || t1.Y == t0.Y;
                int segmentHeight = secondHalf ? (int)(t2.Y - t1.Y) : (int)(t1.Y - t0.Y);
                float alpha = (float)i / totalHeight;
                float beta = (float)(i - (secondHalf ? t1.Y - t0.Y : 0)) / segmentHeight;
                Vector2 a = t0 + (t2 - t0) * alpha;
                Vector2 b = secondHalf ? t1 + (t2 - t1) * beta : t0 + (t1 - t0) * beta;
                if (a.X > b.X) { Swap(ref a, ref b); }
                for (int j = (int)a.X; j <= b.X; j++)
                {
                    image[j, (int)t0.Y + i] = color;

                }
            }
            */
        }

        // Bresenham’s Line Drawing Algorithm
        static void DrawLine(int x0, int y0, int x1, int y1, ref Image<Rgba32> image, Rgba32 color)
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
                if (steep)
                    image[y, x] = color;
                else
                    image[x, y] = color;

                error -= dy;
                if (error < 0)
                {
                    y += yStep;
                    error += dx;
                }
            }
        }

        // Save frame to tga file
        static void SaveFrame(ref Image<Rgba32> image)
        {
            image.SaveAsTga("./out.tga");
        }

        // Swap two number
        static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }

        // Calculate barycentric coordinates
        static Vector3 GetBarycentric(Vector2[] pts, Vector2 p)
        {
            float xa = pts[0].X;
            float ya = pts[0].Y;
            float xb = pts[1].X;
            float yb = pts[1].Y;
            float xc = pts[2].X;
            float yc = pts[2].Y;
            float x = p.X;
            float y = p.Y;

            float gamma = ((ya - yb) * x + (xb - xa) * y + xa * yb - xb * ya) / ((ya - yb) * xc + (xb - xa) * yc + xa * yb - xb * ya);
            float beta = ((ya - yc) * x + (xc - xa) * y + xa * yc - xc * ya) / ((ya - yc) * xb + (xc - xa) * yb + xa * yc - xc * ya);
            float alpha = 1 - gamma - beta;

            return new Vector3(alpha, beta, gamma);
        }
    }
}