using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using TinyRenderer_CSharp.Libs;

namespace TinyRenderer_CSharp.Pipeline
{
    public class Renderer
    {
        readonly Preprocessor frame;
        readonly float[,] zBuffer;
        readonly float[,] shadowBuffer;


        public Renderer(ref Preprocessor segFrame)
        {
            frame = segFrame;

            zBuffer = new float[frame.width, frame.height];
            shadowBuffer = new float[frame.width, frame.height];
            for (int i = 0; i < frame.width; i++)
            {
                for (int j = 0; j < frame.height; j++)
                {
                    zBuffer[i, j] = float.MinValue;
                    shadowBuffer[i, j] = float.MinValue;
                }
            }
        }

        public void Render()
        {
            Image<Rgba32> texture = Texture.LoadTexture(Directory.GetCurrentDirectory() + @"/Resources/obj/african_head_diffuse.tga");
            Image<Rgba32> normal = Texture.LoadTexture(Directory.GetCurrentDirectory() + @"/Resources/obj/african_head_nm_tangent.tga");
            Image<Rgba32> specular = Texture.LoadTexture(Directory.GetCurrentDirectory() + @"/Resources/obj/african_head_spec.tga");

            Vector3[] worldCoord = new Vector3[3];
            Vector3[] screenCoord = new Vector3[3];
            Vector3[] shadowBufferCoord = new Vector3[3];
            Vector2[] uv = new Vector2[3];
            Vector3[] vNormal = new Vector3[3];

            if (frame.modelFile != null)
            {
                foreach (var triangle in frame.modelFile)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        shadowBufferCoord[i] = frame.gLight.GetScreenCoord(triangle[i].Vertex);
                    }
                    ShadowBuffering(ref shadowBufferCoord);
                }

                foreach (var triangle in frame.modelFile)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        worldCoord[i] = triangle[i].Vertex;
                        screenCoord[i] = frame.gCam.GetScreenCoord(triangle[i].Vertex);
                        uv[i] = triangle[i].UV;
                        vNormal[i] = triangle[i].Normal;
                    }
                    Draw(ref screenCoord, ref worldCoord, ref uv, ref vNormal, ref texture, ref normal, ref specular);
                }
            }
        }

        void ShadowBuffering(ref Vector3[] shadowCoord)
        {
            Vector2 bboxMin = new(frame.width - 1, frame.height - 1);
            Vector2 bboxMax = new(0, 0);

            bboxMin.X = (new float[] { bboxMin.X, shadowCoord[0].X, shadowCoord[1].X, shadowCoord[2].X }).Min();
            bboxMin.Y = (new float[] { bboxMin.Y, shadowCoord[0].Y, shadowCoord[1].Y, shadowCoord[2].Y }).Min();
            bboxMax.X = (new float[] { bboxMax.X, shadowCoord[0].X, shadowCoord[1].X, shadowCoord[2].X }).Max();
            bboxMax.Y = (new float[] { bboxMax.Y, shadowCoord[0].Y, shadowCoord[1].Y, shadowCoord[2].Y }).Max();

            for (int i = (int)bboxMin.X; i <= bboxMax.X; i++)
            {
                for (int j = (int)bboxMin.Y; j <= bboxMax.Y; j++)
                {
                    Vector3 p = new(i, j, 0);
                    Vector3 baryCoord = Geometric.GetBarycentric(shadowCoord, p);

                    if (baryCoord.X < 0 || baryCoord.Y < 0 || baryCoord.Z < 0) continue;

                    float shadowInterpolation = baryCoord.X * shadowCoord[0].Z + baryCoord.Y * shadowCoord[1].Z + baryCoord.Z * shadowCoord[2].Z;
                    if (shadowInterpolation < shadowBuffer[i, j]) continue;
                    shadowBuffer[i, j] = shadowInterpolation;
                }
            }
        }

        void Draw(ref Vector3[] screenCoord, ref Vector3[] worldCoord, ref Vector2[] uv, ref Vector3[] vNormal, ref Image<Rgba32> texture, ref Image<Rgba32> normal, ref Image<Rgba32> specular)
        {
            Vector2 bboxMin = new(frame.width - 1, frame.height - 1);
            Vector2 bboxMax = new(0, 0);

            bboxMin.X = (new float[] { bboxMin.X, screenCoord[0].X, screenCoord[1].X, screenCoord[2].X }).Min();
            bboxMin.Y = (new float[] { bboxMin.Y, screenCoord[0].Y, screenCoord[1].Y, screenCoord[2].Y }).Min();
            bboxMax.X = (new float[] { bboxMax.X, screenCoord[0].X, screenCoord[1].X, screenCoord[2].X }).Max();
            bboxMax.Y = (new float[] { bboxMax.Y, screenCoord[0].Y, screenCoord[1].Y, screenCoord[2].Y }).Max();


            for (int i = (int)bboxMin.X; i <= bboxMax.X; i++)
            {
                for (int j = (int)bboxMin.Y; j <= bboxMax.Y; j++)
                {
                    Vector3 p = new(i, j, 0);
                    Vector3 baryCoord = Geometric.GetBarycentric(screenCoord, p);

                    if (baryCoord.X < 0 || baryCoord.Y < 0 || baryCoord.Z < 0) continue;

                    // Z-interpolation
                    float zInterpolation = baryCoord.X * screenCoord[0].Z + baryCoord.Y * screenCoord[1].Z + baryCoord.Z * screenCoord[2].Z;
                    if (zInterpolation < zBuffer[i, j]) continue;
                    zBuffer[i, j] = zInterpolation;


                    // Shadow mapping
                    Vector3 pShadow = baryCoord.X * screenCoord[0] + baryCoord.Y * screenCoord[1] + baryCoord.Z * screenCoord[2];
                    Matrix4x4 pShadowM = new() { M11 = pShadow.X, M21 = pShadow.Y, M31 = pShadow.Z, M41 = 1.0f };
                    Matrix4x4 lightMvp = frame.gLight.GetMvp(); Matrix4x4 camMvp = frame.gCam.GetMvp();
                    if (!Matrix4x4.Invert(camMvp, out Matrix4x4 inCam)) continue;
                    pShadowM = inCam * pShadowM; pShadowM = lightMvp * pShadowM;
                    pShadowM.M11 /= pShadowM.M41; pShadowM.M21 /= pShadowM.M41; pShadowM.M31 /= pShadowM.M41;
                    float shadow = 0.3f + 0.7f * (shadowBuffer[(int)pShadowM.M11, (int)pShadowM.M21] < pShadowM.M31 + 5 ? 1f : 0f);

                    // Draw pixel
                    FragmentPara para = new(ref texture, ref normal, ref specular, ref screenCoord, ref worldCoord, ref uv, ref baryCoord, ref frame.lightDir, ref frame.cameraPos, ref vNormal, ref zInterpolation, ref shadow);
                    Rgba32 color = frame.shader.GetFragment(ref para);

                    frame.frameBuffer[i, j] = color;
                }
            }
        }
    }
}
