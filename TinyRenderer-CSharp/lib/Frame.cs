using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Numerics;

namespace TinyRenderer_CSharp.Lib
{
    public class Frame
    {
        readonly int width;
        readonly int height;
        readonly bool useTexture;

        Vector3 lightDir;

        Image<Rgba32> framebuffer;
        readonly IReadOnlyCollection<TriangleInfo> model;
        Image<Rgba32> texture;
        float[] zBuffer;

        public Frame(ref IReadOnlyCollection<TriangleInfo> model, ref Image<Rgba32> texture, int width, int height, Vector3 lightDir, Vector3 cameraPos, bool useTexture = true, int depth = 255)
        {
            this.width = width;
            this.height = height;
            this.useTexture = useTexture;

            // Initiate frame
            framebuffer = new(this.width, this.height);

            // Light & camera
            this.lightDir = lightDir;
            Camera camera = new(cameraPos, Vector3.UnitY, Vector3.Zero - cameraPos);

            // Setups
            zBuffer = new float[width * height];
            for (int i = 0; i < width * height; i++) zBuffer[i] = float.MinValue;
            this.model = model;
            this.texture = texture;
            MvpTools.SetMvpTools(camera, width, height, depth);
        }

        public void RenderFrame()
        {
            Vector3[] screenCoord = new Vector3[3];
            Vector3[] worldCoord = new Vector3[3];
            Vector2[] uv = new Vector2[3];
            Vector3[] normal = new Vector3[3];

            if (model != null)
            {
                foreach (var item in model)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        worldCoord[i] = item[i].Vertex;
                        screenCoord[i] = MvpTools.GetScreenCoord(worldCoord[i]);
                        uv[i] = item[i].UV;
                        normal[i] = item[i].Normal;
                    }

                    DrawTriangle(ref screenCoord, ref uv, ref normal, ref zBuffer, ref framebuffer, ref texture);
                }
            }
        }

        void DrawTriangle(ref Vector3[] screenCoord, ref Vector2[] uv, ref Vector3[] normal, ref float[] zBuffer, ref Image<Rgba32> image, ref Image<Rgba32> texture)
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
                    Vector3 baryCoord = GetBarycentric(screenCoord, p);

                    if (baryCoord.X < -0.001 || baryCoord.Y < -0.001 || baryCoord.Z < -0.001) continue;

                    // Z-interpolation
                    float zInterpolation = baryCoord.X * screenCoord[0].Z + baryCoord.Y * screenCoord[1].Z + baryCoord.Z * screenCoord[2].Z;

                    // Phong shading
                    Vector3 pNormal = normal[0] * baryCoord.X + normal[1] * baryCoord.Y + normal[2] * baryCoord.Z;
                    float intensity = Vector3.Dot(pNormal, lightDir);
                    if (intensity < -0.001) intensity = 0;

                    // Flat Shading
                    //Vector3 pNormal = (normal[0] + normal[1] + normal[2]) / 3;
                    //float intensity = Vector3.Dot(pNormal, lightDir);
                    //if (intensity < 0) intensity = 0;

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
                        Rgba32 color = Texture.GetColor(texture, pUV);
                        if (!useTexture)
                        {
                            color = Color.White;
                        }
                        image[i, j] = GetColor(color, intensity);
                    }
                }
            }
        }

        public void SaveFrame(string curDir, string name = "out")
        {
            // Flip the frame
            framebuffer.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            framebuffer.SaveAsTga(curDir + "/" + name + ".tga");
        }

        static Rgba32 GetColor(Rgba32 color, float intensity)
        {
            color.R = (byte)(color.R * intensity);
            color.G = (byte)(color.G * intensity);
            color.B = (byte)(color.B * intensity);

            return color;
        }

        static Vector3 GetBarycentric(Vector3[] screenCoord, Vector3 p)
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
    }
}

