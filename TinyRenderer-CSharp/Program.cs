using System.Numerics;
using TinyRenderer_CSharp.Libs;
using TinyRenderer_CSharp.Pipeline;

namespace TinyRenderer_CSharp
{
    class Program
    {
        static void Main()
        {
            // Render settings
            Vector2 screenSize = new(2000, 2000);
            Vector3 lightDir = new(1, 1, 1);
            Vector3 cameraPos = new(1, 1, 3);
            // Shader & Texture
            IShader shader = new Shaders.Phong(true);

            // Rendering
            Preprocessor frame = new(screenSize, lightDir, cameraPos, shader);
            Renderer renderer = new(ref frame); renderer.Render();
            PostProcessing.Save(ref frame);
        }
    }
}