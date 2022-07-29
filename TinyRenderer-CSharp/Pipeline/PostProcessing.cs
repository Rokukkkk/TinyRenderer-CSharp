using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace TinyRenderer_CSharp.Pipeline
{
    public static class PostProcessing
    {
        public static void Save(ref Preprocessor frame)
        {
            // Flip the frame
            frame.frameBuffer.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            frame.frameBuffer.SaveAsTga(Directory.GetCurrentDirectory() + "/out.tga");
        }
    }
}

