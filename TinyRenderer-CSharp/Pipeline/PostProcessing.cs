using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace TinyRenderer_CSharp.Pipeline
{
    public static class PostProcessing
    {
        public static void Save(ref Preprocessor frame)
        {
            // Flip the frame
            frame.frameBuffer.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            frame.frameBuffer.SaveAsPng(Directory.GetCurrentDirectory() + "/out.png");
        }

        public static void SSAO(ref Preprocessor frame)
        {
            // SSAO
            for (int i = 0; i < frame.width; i++)
            {
                for (int j = 0; j < frame.height; j++)
                {
                    if (frame.zBuffer[i, j] < -1e5) continue;
                    float sum = 0;
                    for (float a = 0; a < Math.PI * 2 - 1e-4; a += (float)(Math.PI / 4))
                    {
                        sum += (float)(Math.PI / 2) - GetMaxEleAngle(ref frame, new Vector2(i, j), new Vector2((float)Math.Cos(a), (float)Math.Sin(a)));
                    }
                    sum /= (float)(Math.PI / 2) * 8;
                    sum = (float)Math.Pow(sum, 100);
                    Rgba32 buffer = frame.frameBuffer[i, j];
                    buffer.R = (byte)(buffer.R * sum * 1.2f); buffer.G = (byte)(buffer.G * sum * 1.2f); buffer.B = (byte)(buffer.B * sum * 1.2f);
                    frame.frameBuffer[i, j] = buffer;
                }
            }
        }

        static float GetMaxEleAngle(ref Preprocessor frame, Vector2 p, Vector2 dir)
        {
            float maxEleAngle = 0f;
            for (int i = 0; i < 1000; i++)
            {
                Vector2 cur = p + dir * i;
                if (cur.X >= frame.width || cur.Y >= frame.height || cur.X <= 0 || cur.Y <= 0) return maxEleAngle;

                float distance = Vector2.Distance(p, cur);
                if (distance < 1) continue;
                float ele = frame.zBuffer[(int)cur.X, (int)cur.Y] - frame.zBuffer[(int)p.X, (int)p.Y];
                maxEleAngle = Math.Max(maxEleAngle, (float)Math.Atan(ele / distance));
            }
            return maxEleAngle;
        }
    }
}