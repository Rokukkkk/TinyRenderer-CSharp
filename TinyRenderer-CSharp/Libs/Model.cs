using System.Globalization;
using System.Numerics;

namespace TinyRenderer_CSharp.Libs
{
    public static class Model
    {
        private const string VertexTag = "v";
        private const string NormalTag = "vn";
        private const string UVTag = "vt";
        private const string TriangleTag = "f";

        private static readonly char[] Separators = { '/', ' ' };

        public static IReadOnlyCollection<TriangleInfo> LoadModel(string path)
        {
            List<TriangleInfo> triangles = new();

            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<Vector2> uv = new();

            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var items = line.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

                    switch (items.FirstOrDefault())
                    {
                        case VertexTag:
                            vertices.Add(new Vector3(
                                Convert.ToSingle(items[1], CultureInfo.InvariantCulture.NumberFormat),
                                Convert.ToSingle(items[2], CultureInfo.InvariantCulture.NumberFormat),
                                Convert.ToSingle(items[3], CultureInfo.InvariantCulture.NumberFormat)));
                            break;
                        case NormalTag:
                            normals.Add(new Vector3(
                                Convert.ToSingle(items[1], CultureInfo.InvariantCulture.NumberFormat),
                                Convert.ToSingle(items[2], CultureInfo.InvariantCulture.NumberFormat),
                                Convert.ToSingle(items[3], CultureInfo.InvariantCulture.NumberFormat)));
                            break;
                        case UVTag:
                            uv.Add(new Vector2(
                                Convert.ToSingle(items[1], CultureInfo.InvariantCulture.NumberFormat),
                                Convert.ToSingle(items[2], CultureInfo.InvariantCulture.NumberFormat)));
                            break;
                        case TriangleTag:
                            var triangle = new TriangleInfo(
                                new TriangleInfo.VertexInfo(
                                    vertices[Convert.ToInt32(items[1]) - 1],
                                    uv[Convert.ToInt32(items[2]) - 1],
                                    normals[Convert.ToInt32(items[3]) - 1]),
                                new TriangleInfo.VertexInfo(
                                    vertices[Convert.ToInt32(items[4]) - 1],
                                    uv[Convert.ToInt32(items[5]) - 1],
                                    normals[Convert.ToInt32(items[6]) - 1]),
                                new TriangleInfo.VertexInfo(
                                    vertices[Convert.ToInt32(items[7]) - 1],
                                    uv[Convert.ToInt32(items[8]) - 1],
                                    normals[Convert.ToInt32(items[9]) - 1])
                                  );

                            triangles.Add(triangle);
                            break;
                    }
                }
            }
            return triangles;
        }
    }
}