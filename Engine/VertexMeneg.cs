using Veldrid;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

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

    public struct Square
    {
        public float Width { get; private set; }
        public float Height { get; private set; }
        public int Z_Index { get; private set; }
        public Vector3 Rotations { get; private set; }
        public Vector3 Scale { get; private set; }
        public Vector3 Translation { get; private set; }
        public Vector4 BagroundColor { get; private set; }
        public Matrix4x4 Model {  get; private set; }
        public int TextureIndex { get; private set; }
        public float Time {  get; private set; }
        public float AnimationSpeed { get; private set; }
        public bool UpdateModelMatrix { get; private set; }
        private Vector3 ScaleDef = new Vector3(1,1,1);

        public Square(
            float Width = 0,
            float Height = 0,
            int Z_Index = 1,
            Vector3 Rotations = new Vector3(),
            Vector3 Scale = new Vector3(),
            Vector3 Translation = new Vector3(),
            Vector4 BagroundColor = new Vector4(),
            int TextureIndex = -1,
            float Time = 0.0f,
            float AnimationSpeed = 0.0f,
            bool UpdateModelMatrix = false
            )
        {
            this.Width = Width;
            this.Height = Height;
            this.Z_Index = Z_Index;
            this.Rotations = Rotations;
            this.Scale = Scale;
            this.Translation = Translation;
            this.TextureIndex = TextureIndex;
            this.BagroundColor = BagroundColor;
            this.Time = Time;
            this.AnimationSpeed = AnimationSpeed;
            this.UpdateModelMatrix = UpdateModelMatrix;
            this.Model = MatrixModel(Scale, Rotations, Translation);
        }
        public void Update(Square update)
        {
            this.Width = update.Width == 0 ? this.Width : update.Width;
            this.Height = update.Height == 0 ? this.Height : update.Height;
            this.Z_Index = update.Z_Index == 0 ? this.Z_Index : update.Z_Index;
            this.Rotations = (update.Rotations == Vector3.Zero) ? this.Rotations : update.Rotations ;
            this.Scale = (update.Scale == Vector3.Zero) ? this.Scale : update.Scale;
            this.Translation = (update.Translation == Vector3.Zero) ? this.Translation : update.Translation;
            this.TextureIndex = update.TextureIndex == -1 ? this.TextureIndex : update.TextureIndex;
            this.BagroundColor = update.BagroundColor == Vector4.Zero ? this.BagroundColor: update.BagroundColor;
            this.Time = update.Time;
            this.UpdateModelMatrix = update.UpdateModelMatrix;
            this.AnimationSpeed = update.AnimationSpeed;

            if (update.UpdateModelMatrix)
                this.Model = MatrixModel(update.Scale, update.Rotations, update.Translation);
            
        }

        public void Animation(float Time = 0.0f, float AnimationsSeed = 0.0f)
        {
            this.Time = Time;
            this.AnimationSpeed = AnimationsSeed;
            this.Model = MatrixModel(this.Scale, this.Rotations, this.Translation, Time, AnimationsSeed);
        }

        public static Matrix4x4 MatrixModel(Vector3 Scale, Vector3 Rotations, Vector3 Translation, float Time = 0.0f, float AnimationsSeed = 0.0f)
        {
            Matrix4x4 rotationMatrixX;
            Matrix4x4 rotationMatrixY;
            Matrix4x4 rotationMatrixZ;

            if (Time == 0.0f || AnimationsSeed == 0.0f)
            {
                rotationMatrixX = Matrix4x4.CreateRotationX(Rotations.X);
                rotationMatrixY = Matrix4x4.CreateRotationY(Rotations.Y);
                rotationMatrixZ = Matrix4x4.CreateRotationZ(Rotations.Z);
            }
            else
            {
                rotationMatrixX = Matrix4x4.CreateRotationX(Rotations.X * Time * AnimationsSeed);
                rotationMatrixY = Matrix4x4.CreateRotationY(Rotations.Y * Time * AnimationsSeed);
                rotationMatrixZ = Matrix4x4.CreateRotationZ(Rotations.Z * Time * AnimationsSeed);
            }

            Matrix4x4 modelMatrix = Matrix4x4.CreateScale(Scale) * (rotationMatrixX * rotationMatrixY * rotationMatrixZ) * Matrix4x4.CreateTranslation(Translation);

            return modelMatrix;
        }
    }
    internal class VertexMeneg
    {
        private VertexLayoutDescription LayoutDescriptionVertex = new VertexLayoutDescription(
            new VertexElementDescription("aTextureCoordinate", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("aPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("aColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlo", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlt", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlth", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlf", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        
        public List<Vertex[]> vertex { get; private set; }

        public List<ushort[]> indices { get; private set; }

        public VertexLayoutDescription GetLayoutVertex() {return this.LayoutDescriptionVertex;}
        public VertexMeneg() { 
            this.vertex = new List<Vertex[]>();
            this.indices = new List<ushort[]>();
        }

        public void NewVertexSquare(Square newObj)
        {

            vertex.Add( new Vertex[]
        {
            new Vertex(new Vector3(newObj.Width/2, +newObj.Height/2, -newObj.Z_Index), newObj.BagroundColor,
                new Vector2(0.0f, 1.0f),
                new Vector4(newObj.Model.M11, newObj.Model.M12, newObj.Model.M13, newObj.Model.M14),
                new Vector4(newObj.Model.M21, newObj.Model.M22, newObj.Model.M23, newObj.Model.M24),
                new Vector4(newObj.Model.M31, newObj.Model.M32, newObj.Model.M33, newObj.Model.M34),
                new Vector4(newObj.Model.M41, newObj.Model.M42, newObj.Model.M43, newObj.Model.M44)),

            new Vertex(new Vector3(newObj.Width/2, -newObj.Height/2, -newObj.Z_Index), newObj.BagroundColor,
                new Vector2(1.0f, 1.0f),
                new Vector4(newObj.Model.M11, newObj.Model.M12, newObj.Model.M13, newObj.Model.M14),
                new Vector4(newObj.Model.M21, newObj.Model.M22, newObj.Model.M23, newObj.Model.M24),
                new Vector4(newObj.Model.M31, newObj.Model.M32, newObj.Model.M33, newObj.Model.M34),
                new Vector4(newObj.Model.M41, newObj.Model.M42, newObj.Model.M43, newObj.Model.M44)),

            new Vertex(new Vector3(-newObj.Width/2, -newObj.Height/2, -newObj.Z_Index), newObj.BagroundColor,
                new Vector2(1.0f, 0.0f),
                new Vector4(newObj.Model.M11, newObj.Model.M12, newObj.Model.M13, newObj.Model.M14),
                new Vector4(newObj.Model.M21, newObj.Model.M22, newObj.Model.M23, newObj.Model.M24),
                new Vector4(newObj.Model.M31, newObj.Model.M32, newObj.Model.M33, newObj.Model.M34),
                new Vector4(newObj.Model.M41, newObj.Model.M42, newObj.Model.M43, newObj.Model.M44)),

            new Vertex(new Vector3(-newObj.Width/2, newObj.Height/2, -newObj.Z_Index), newObj.BagroundColor,
                new Vector2(0.0f, 0.0f),
                new Vector4(newObj.Model.M11, newObj.Model.M12, newObj.Model.M13, newObj.Model.M14),
                new Vector4(newObj.Model.M21, newObj.Model.M22, newObj.Model.M23, newObj.Model.M24),
                new Vector4(newObj.Model.M31, newObj.Model.M32, newObj.Model.M33, newObj.Model.M34),
                new Vector4(newObj.Model.M41, newObj.Model.M42, newObj.Model.M43, newObj.Model.M44))
            });

            ushort[] arrayDraw = new ushort[6];
            if (this.indices.Count != 0)
            {
                int FirstIndexVertex = this.indices[this.indices.Count - 1][4];
                arrayDraw[0] = (ushort)(FirstIndexVertex + 1);
                arrayDraw[1] = (ushort)(FirstIndexVertex + 2);
                arrayDraw[2] = (ushort)(FirstIndexVertex + 3);
                arrayDraw[3] = (ushort)(FirstIndexVertex + 3);
                arrayDraw[4] = (ushort)(FirstIndexVertex + 4);
                arrayDraw[5] = (ushort)(FirstIndexVertex + 1);
                this.indices.Add(arrayDraw);
                return;
            }
            arrayDraw[0] = 0;
            arrayDraw[1] = 1;
            arrayDraw[2] = 2;
            arrayDraw[3] = 2;
            arrayDraw[4] = 3;
            arrayDraw[5] = 0;
            this.indices.Add(arrayDraw);
        }
    }
}
