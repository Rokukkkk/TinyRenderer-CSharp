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
        public const int depth = 255;

        static readonly bool useTexture = true;  // Use texture or not
        static Vector3 lightDir = Vector3.Normalize(new Vector3(1, -1, 1));
        public static Vector3 cameraPos = new(1, 1, 3);
        public static Vector3 center = Vector3.Zero;

        static void Main()
        {
            // Initiate frame
            Image<Rgba32> image = new(width, height);

            // Load the model
            var model = Model.LoadModel(@"./obj/african_head.obj");
            var texture = Texture.LoadTexture(@"./obj/african_head_diffuse.tga");

            // MVP
            Matrix4x4 modelViewMatrix = MvpTools.GetModelViewMatrix();
            Matrix4x4 projectionMartix = MvpTools.GetProjectionMatrix();
            Matrix4x4 viewportMartix = MvpTools.GetViewportMatrix(depth, width / 8, height / 8, width * 3 / 4, height * 3 / 4);

            // Z-Buffer
            float[] zBuffer = new float[width * height];
            for (int i = 0; i < width * height; i++) zBuffer[i] = float.MinValue;

            // Render
            Vector3[] screenCoord = new Vector3[3];
            Vector3[] worldCoord = new Vector3[3];
            Vector2[] uv = new Vector2[3];
            Vector3[] normal = new Vector3[3];

            foreach (var item in model)
            {
                for (int i = 0; i < 3; i++)
                {
                    worldCoord[i] = item[i].Vertex;
                    screenCoord[i] = Homo2Vertices(viewportMartix * MvpTools.GetProjectionDivision(projectionMartix * modelViewMatrix * Local2Homo(ref worldCoord[i])));
                    uv[i] = item[i].UV;
                    normal[i] = item[i].Normal;
                }

                DrawTriangle(ref screenCoord, ref uv, ref normal, ref zBuffer, ref image, ref texture);
            }

            //Flip the image upside down, and saving
            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            SaveFrame(ref image);

        }

        static void DrawTriangle(ref Vector3[] screenCoord, ref Vector2[] uv, ref Vector3[] normal, ref float[] zBuffer, ref Image<Rgba32> image, ref Image<Rgba32> texture)
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

                    if (baryCoord.X < -0.001 || baryCoord.Y < -0.001 || baryCoord.Z < -0.001) continue;

                    // Z-interpolation
                    float zInterpolation = baryCoord.X * screenCoord[0].Z + baryCoord.Y * screenCoord[1].Z + baryCoord.Z * screenCoord[2].Z;

                    // Phong shading
                    Vector3 pNormal = normal[0] * baryCoord.X + normal[1] * baryCoord.Y + normal[2] * baryCoord.Z;
                    float intensity = Vector3.Dot(pNormal, lightDir);
                    if (intensity < 0) continue;

                    // Flat Shading
                    //Vector3 pNormal = (normal[0] + normal[1] + normal[2]) / 3;
                    //float intensity = Vector3.Dot(pNormal, lightDir);
                    //if (intensity < 0) continue;

                    // Gouraud Shading
                    //float intensity1 = Vector3.Dot(normal[0], lightDir);
                    //float intensity2 = Vector3.Dot(normal[1], lightDir);
                    //float intensity3 = Vector3.Dot(normal[2], lightDir);
                    //if (intensity1 < 0 || intensity2 < 0 || intensity3 < 0) continue;
                    //Rgba32 color1 = GetColor(texture.GetColor(uv[0]), Vector3.Dot(normal[0], lightDir));
                    //Rgba32 color2 = GetColor(texture.GetColor(uv[1]), Vector3.Dot(normal[1], lightDir));
                    //Rgba32 color3 = GetColor(texture.GetColor(uv[2]), Vector3.Dot(normal[2], lightDir));
                    //byte pR = (byte)(color1.R * baryCoord.X + color2.R * baryCoord.Y + color3.R * baryCoord.Z);
                    //byte pG = (byte)(color1.G * baryCoord.X + color2.G * baryCoord.Y + color3.G * baryCoord.Z);
                    //byte pB = (byte)(color1.B * baryCoord.X + color2.B * baryCoord.Y + color3.B * baryCoord.Z);
                    //Rgba32 pColor = new(pR, pG, pB);

                    // UV-interpolation (Texutre)
                    Vector2 pUV = uv[0] * baryCoord.X + uv[1] * baryCoord.Y + uv[2] * baryCoord.Z;


                    if (zInterpolation >= zBuffer[i + j * width])
                    {
                        zBuffer[i + j * width] = zInterpolation;
                        Rgba32 color = texture.GetColor(pUV);
                        if (!useTexture)
                        {
                            color = Color.White;
                        }
                        image[i, j] = GetColor(color, intensity);

                        // Gouraud shading draw caller
                        //image[i, j] = pColor;
                    }
                }
            }
        }

        // Save frame to tga file
        static void SaveFrame(ref Image<Rgba32> image)
        {
            image.SaveAsTga("./out.tga");
        }

        // Calculate color
        static Rgba32 GetColor(Rgba32 color, float intensity)
        {
            color.R = (byte)(color.R * intensity);
            color.G = (byte)(color.G * intensity);
            color.B = (byte)(color.B * intensity);

            return color;
        }

        // Calculate barycentric coordinates
        static Vector3 GetBarycentric(ref Vector3[] screenCoord, Vector3 p)
        {
            Vector3 v0 = screenCoord[1] - screenCoord[0];
            Vector3 v1 = screenCoord[2] - screenCoord[0];
            Vector3 v2 = p - screenCoord[0];
            float den = v0.X * v1.Y - v1.X * v0.Y;
            float v = (v2.X * v1.Y - v1.X * v2.Y) / den;
            float w = (v0.X * v2.Y - v2.X * v0.Y) / den;
            float u = 1.0f - v - w;

            return new Vector3(u, v, w);
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

        static Vector3 Homo2Vertices(Matrix4x4 m)
        {
            return new Vector3(m.M11, m.M21, m.M31);
        }
    }

    // Tools for MVP transformation
    class MvpTools
    {
        static Camera camera = new(Program.cameraPos, Vector3.UnitY, Program.center - Program.cameraPos);

        public static Matrix4x4 GetModelViewMatrix()
        {
            Matrix4x4 r = Matrix4x4.Identity;
            Matrix4x4 t = Matrix4x4.Identity;

            r.M11 = camera.Right.X;
            r.M21 = camera.Up.X;
            r.M31 = -camera.Front.X;
            t.M14 = -camera.Position.X;
            r.M12 = camera.Right.Y;
            r.M22 = camera.Up.Y;
            r.M32 = -camera.Front.Y;
            t.M24 = -camera.Position.Y;
            r.M13 = camera.Right.Z;
            r.M23 = camera.Up.Z;
            r.M33 = -camera.Front.Z;
            t.M34 = -camera.Position.Z;

            return r * t;
        }

        public static Matrix4x4 GetProjectionMatrix()
        {
            Matrix4x4 projection = Matrix4x4.Identity;
            projection.M43 = -1 / camera.Position.Z;

            return projection;
        }

        public static Matrix4x4 GetProjectionDivision(Matrix4x4 m)
        {
            m.M11 /= m.M41;
            m.M21 /= m.M41;
            m.M31 /= m.M41;
            m.M41 = 1.0f;

            return m;
        }

        public static Matrix4x4 GetViewportMatrix(int depth, int x, int y, int w, int h)
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
    }
}