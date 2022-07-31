using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using TinyRenderer_CSharp.Libs;

namespace TinyRenderer_CSharp.Pipeline
{
    public class Preprocessor
    {
        public int width;
        public int height;
        public Vector3 lightDir;
        public Vector3 cameraPos;

        public Image<Rgba32> frameBuffer;
        public Camera camera;
        public Camera light;

        public IReadOnlyCollection<TriangleInfo>? modelFile;
        public Geometric gCam;
        public Geometric gLight;

        public IShader shader;

        public float[,] zBuffer;
        public float[,] shadowBuffer;

        public Preprocessor(Vector2 screenSize, Vector3 lightDir, Vector3 cameraPos, IShader shader)
        {
            // Width & Height
            width = (int)screenSize.X; height = (int)screenSize.Y;
            frameBuffer = new(width, height);

            // Light & Camera
            this.lightDir = Vector3.Normalize(lightDir);
            this.cameraPos = cameraPos;
            camera = new(cameraPos, Vector3.UnitY, Vector3.Zero - cameraPos);
            light = new(lightDir, Vector3.UnitY, Vector3.Zero - lightDir);

            // Model & Geometric
            modelFile = Model.LoadModel(Directory.GetCurrentDirectory() + @"/Resources/obj/african_head.obj");
            gCam = new(camera, width, height);
            gLight = new(light, width, height);

            // Shader
            this.shader = shader;

            // Buffers
            zBuffer = new float[width, height];
            shadowBuffer = new float[width, height];
        }
    }
}

