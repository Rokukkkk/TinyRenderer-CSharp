using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TinyRenderer_CSharp.Lib;
using TinyRenderer_CSharp.Shaders;

namespace TinyRenderer_CSharp
{
    class Program
    {
        static void Main()
        {
            string curDir = Directory.GetCurrentDirectory();
            IReadOnlyCollection<TriangleInfo> model = Model.LoadModel(curDir + @"/obj/african_head.obj");
            Image<Rgba32> texture = Texture.LoadTexture(curDir + @"/obj/african_head_diffuse.tga");

            int width = 2000;
            int height = 2000;
            bool useTexture = true;
            Vector3 lightDir = Vector3.Normalize(new(1, -1, 1));
            Vector3 cameraPos = new(1, 1, 2);

            // Initiate the frame
            Frame frame = new(ref model, ref texture, width, height, lightDir, cameraPos, useTexture);

            // Render the frame
            frame.RenderFrame();

            // Save the frame
            frame.SaveFrame(curDir);
        }
    }
}