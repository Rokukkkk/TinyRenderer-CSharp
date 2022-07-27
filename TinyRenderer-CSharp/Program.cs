using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TinyRenderer_CSharp.Lib;

namespace TinyRenderer_CSharp
{
    class Program
    {
        static void Main()
        {
            string curDir = Directory.GetCurrentDirectory();

            // Render settings
            int width = 2000;
            int height = 2000;
            Vector3 lightDir = Vector3.Normalize(new(1, -1, 1));
            Vector3 cameraPos = new(1, 1, 2);

            bool useTexture = true;
            IShader shader = new Shaders.Phong(useTexture);

            IReadOnlyCollection<TriangleInfo> model = Model.LoadModel(curDir + @"/obj/african_head.obj");
            Image<Rgba32> texture = Texture.LoadTexture(curDir + @"/obj/african_head_diffuse.tga");

            // Initiate the frame
            Frame frame = new(ref model, ref texture, width, height, lightDir, cameraPos);

            // Render the frame
            frame.RenderFrame(ref shader);

            // Save the frame
            frame.SaveFrame(curDir);
        }
    }
}