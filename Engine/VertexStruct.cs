using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using System.Numerics;
using System.Text;
using System.Runtime.CompilerServices;

namespace VeldridAPPvk.Engine
{
    struct Vertex
    {
        public Vector2 TextureCoordinate;
        public Vector3 Position;
        public Vector4 Color;
        public Vector4 ModelMlo;
        public Vector4 ModelMlt;
        public Vector4 ModelMlth;
        public Vector4 ModelMlf;

        public Vertex(Vector3 position, Vector4 color, Vector2 textureCoordinate, Vector4 modelMlo, Vector4 modelMlt, Vector4 modelMlth, Vector4 modelMlf)
        {
            TextureCoordinate = textureCoordinate;
            Position = position;
            Color = color;
            ModelMlo = modelMlo;
            ModelMlt = modelMlt;
            ModelMlth = modelMlth;
            ModelMlf = modelMlf;
        }
    }
    internal class VertexStruct
    {
        public VertexLayoutDescription LayoutDescriptionVertex { get {
                return new VertexLayoutDescription(
            new VertexElementDescription("aTextureCoordinate", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("aPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("aColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlo", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlt", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlth", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlf", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
            } }

        public ushort[] indices { get; private set; }
    }
}
