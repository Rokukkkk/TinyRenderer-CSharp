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

        Vector3 lightDir;
        Vector3 cameraPos;

        Image<Rgba32> framebuffer;
        readonly IReadOnlyCollection<TriangleInfo> model;
        float[] zBuffer;

        IShader shader = new Shaders.Flat(false);

        public Frame(ref IReadOnlyCollection<TriangleInfo> model, int width, int height, Vector3 lightDir, Vector3 cameraPos, int depth = 255)
        {
            this.width = width;
            this.height = height;

            // Initiate frame
            framebuffer = new(this.width, this.height);

            // Light & camera
            this.lightDir = lightDir;
            this.cameraPos = cameraPos;
            Camera camera = new(cameraPos, Vector3.UnitY, Vector3.Zero - cameraPos);

            // Setups
            zBuffer = new float[width * height];
            for (int i = 0; i < width * height; i++) zBuffer[i] = float.MinValue;
            this.model = model;
            MvpTools.SetMvpTools(camera, width, height, depth);
        }

        public void RenderFrame(ref IShader shader, ref Image<Rgba32> texture, ref Image<Rgba32> normal, ref Image<Rgba32> specular)
        {
            // Shader
            this.shader = shader;

            Vector3[] screenCoord = new Vector3[3];
            Vector3[] worldCoord = new Vector3[3];
            Vector2[] uv = new Vector2[3];
            Vector3[] vNormal = new Vector3[3];

            if (model != null)
            {
                foreach (var item in model)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        worldCoord[i] = item[i].Vertex;
                        screenCoord[i] = MvpTools.GetScreenCoord(worldCoord[i]);
                        uv[i] = item[i].UV;
                        vNormal[i] = item[i].Normal;
                    }

                    DrawTriangle(ref screenCoord, ref worldCoord, ref uv, ref zBuffer, ref framebuffer, ref vNormal, ref texture, ref normal, ref specular);
                }
            }
        }

        void DrawTriangle(ref Vector3[] screenCoord, ref Vector3[] worldCoord, ref Vector2[] uv, ref float[] zBuffer, ref Image<Rgba32> image, ref Vector3[] vNormal, ref Image<Rgba32> texture, ref Image<Rgba32> normal, ref Image<Rgba32> specular)
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

                    if (baryCoord.X <= -0.001 || baryCoord.Y <= -0.001 || baryCoord.Z <= -0.001) continue;

                    // Z-interpolation
                    float zInterpolation = baryCoord.X * screenCoord[0].Z + baryCoord.Y * screenCoord[1].Z + baryCoord.Z * screenCoord[2].Z;

                    if (zInterpolation < zBuffer[i + j * width]) continue;
                    zBuffer[i + j * width] = zInterpolation;

                    // Draw pixel
                    var color = shader.GetFragment(ref texture, ref normal, ref specular, ref screenCoord, ref worldCoord, ref uv, baryCoord, lightDir, cameraPos, vNormal);
                    image[i, j] = color;
                }
            }
        }

        public void SaveFrame(string curDir, string name = "out")
        {
            // Flip the frame
            framebuffer.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            framebuffer.SaveAsTga(curDir + "/" + name + ".tga");
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