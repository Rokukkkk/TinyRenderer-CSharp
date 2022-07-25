using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Numerics;

namespace TinyRenderer_CSharp
{
    class Program
    {
        const int width = 2000;
        const int height = 2000;
        const int depth = 255;

        static readonly bool useTexture = true;  // Use texture or not
        static Vector3 lightDir = new(0, 0, -1);
        static Vector3 cameraPos = new(0, 0, 3);

        static void Main(string[] args)
        {
            // Initiate frame
            Image<Rgba32> image = new(width, height);

            // Load the model
            var model = Model.LoadModel(@"./obj/african_head.obj");
            var texture = Texture.LoadTexture(@"./obj/african_head_diffuse.tga");

            // Matrix calls
            Matrix4x4 modelMatrix = GetModelMatrix();
            Matrix4x4 viewMartix = GetViewMatrix();
            Matrix4x4 projectionMartix = GetProjectionMatrix();
            Matrix4x4 viewportMartix = GetViewportMatrix(width / 8, height / 8, width * 3 / 4, height * 3 / 4);

            // Z-Buffer
            float[] zBuffer = new float[width * height];
            for (int i = 0; i < width * height; i++) zBuffer[i] = float.MinValue;

            // Render
            Vector3[] screenCoord = new Vector3[3];
            Vector3[] worldCoord = new Vector3[3];
            Vector2[] uv = new Vector2[3];

            foreach (var item in model)
            {
                for (int i = 0; i < 3; i++)
                {
                    worldCoord[i] = item[i].Vertex;
                    screenCoord[i] = Homo2Vertices(viewportMartix * GetProjectionDivision(projectionMartix * viewMartix * modelMatrix * Local2Homo(ref worldCoord[i])));
                    uv[i] = item[i].UV;
                }

                Vector3 normal = Vector3.Cross(worldCoord[2] - worldCoord[0], worldCoord[1] - worldCoord[0]);
                normal = Vector3.Normalize(normal);
                float intensity = Vector3.Dot(normal, lightDir);

                if (intensity > 0)
                {
                    DrawTriangle(ref screenCoord, ref uv, ref zBuffer, ref image, ref texture, intensity);
                }
            }

            //Flip the image upside down, and saving
            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            SaveFrame(ref image);

        }

        static void DrawTriangle(ref Vector3[] screenCoord, ref Vector2[] uv, ref float[] zBuffer, ref Image<Rgba32> image, ref Image<Rgba32> texture, float intensity)
        {
            Vector2 bboxMin = new(width - 1, height - 1);
            Vector2 bboxMax = new(0, 0);

            bboxMin.X = (new float[] { bboxMin.X, screenCoord[0].X, screenCoord[1].X, screenCoord[2].X }).Min();
            bboxMin.Y = (new float[] { bboxMin.Y, screenCoord[0].Y, screenCoord[1].Y, screenCoord[2].Y }).Min();
            bboxMax.X = (new float[] { bboxMax.X, screenCoord[0].X, screenCoord[1].X, screenCoord[2].X }).Max();
            bboxMax.Y = (new float[] { bboxMax.Y, screenCoord[0].Y, screenCoord[1].Y, screenCoord[2].Y }).Max();


            for (int i = (int)bboxMin.X; i <= bboxMax.X; i++)  // Should be "<=bbox" rather than "<", 
            {
                for (int j = (int)bboxMin.Y; j <= bboxMax.Y; j++)  // because it will cause black strip at hi-res
                {
                    Vector3 p = new(i, j, 0);
                    Vector3 baryCoord = GetBarycentric(ref screenCoord, p);
                    Vector2 pUV;
                    if (baryCoord.X < 0 || baryCoord.Y < 0 || baryCoord.Z < 0) continue;

                    // Z-interpolation
                    float zInterpolation = baryCoord.X * screenCoord[0].Z + baryCoord.Y * screenCoord[1].Z + baryCoord.Z * screenCoord[2].Z;
                    pUV = uv[0] * baryCoord.X + uv[1] * baryCoord.Y + uv[2] * baryCoord.Z;
                    if (zInterpolation >= zBuffer[i + j * width])
                    {
                        zBuffer[i + j * width] = zInterpolation;
                        Rgba32 color = texture.GetColor(pUV);
                        if (!useTexture)
                        {
                            color = Color.White;
                        }
                        color.R = (byte)(color.R * intensity);
                        color.G = (byte)(color.G * intensity);
                        color.B = (byte)(color.B * intensity);
                        image[i, j] = color;
                    }
                }
            }
        }

        // Save frame to tga file
        static void SaveFrame(ref Image<Rgba32> image)
        {
            image.SaveAsTga("./out.tga");
        }

        // Calculate barycentric coordinates
        static Vector3 GetBarycentric(ref Vector3[] screenCoord, Vector3 p)
        {
            float xa = screenCoord[0].X;
            float ya = screenCoord[0].Y;
            float xb = screenCoord[1].X;
            float yb = screenCoord[1].Y;
            float xc = screenCoord[2].X;
            float yc = screenCoord[2].Y;
            float x = p.X;
            float y = p.Y;

            float gamma = ((ya - yb) * x + (xb - xa) * y + xa * yb - xb * ya) / ((ya - yb) * xc + (xb - xa) * yc + xa * yb - xb * ya);
            float beta = ((ya - yc) * x + (xc - xa) * y + xa * yc - xc * ya) / ((ya - yc) * xb + (xc - xa) * yb + xa * yc - xc * ya);
            float alpha = 1 - gamma - beta;

            return new Vector3(alpha, beta, gamma);
        }

        // Homogeneous coordinates
        static Matrix4x4 Local2Homo(ref Vector3 local)
        {
            Matrix4x4 m = new();
            m.M11 = local.X;
            m.M21 = local.Y;
            m.M31 = local.Z;
            m.M41 = 1.0f;

            return m;
        }

        // MVP
        static Matrix4x4 GetModelMatrix()
        {
            return Matrix4x4.Identity;
        }

        static Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.Identity;
        }

        static Matrix4x4 GetProjectionMatrix()
        {
            Matrix4x4 projection = Matrix4x4.Identity;
            projection.M43 = -1 / cameraPos.Z;

            return projection;
        }

        static Matrix4x4 GetProjectionDivision(Matrix4x4 m)
        {
            m.M11 /= m.M41;
            m.M21 /= m.M41;
            m.M31 /= m.M41;
            m.M41 = 1.0f;

            return m;
        }

        static Matrix4x4 GetViewportMatrix(int x, int y, int w, int h)
        {
            Matrix4x4 m = Matrix4x4.Identity;
            m.M14 = x + w / 2.0f;
            m.M24 = y + h / 2.0f;
            m.M34 = depth / 2.0f;

            m.M11 = w / 2.0f;
            m.M22 = h / 2.0f;
            m.M33 = depth / 2.0f;

            return m;
        }

        static Vector3 Homo2Vertices(Matrix4x4 m)
        {
            return new Vector3(m.M11, m.M21, m.M31);
        }
    }
}