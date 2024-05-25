using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using System.Numerics;
using System.Text;
using System.Runtime.CompilerServices;

namespace VeldridAPPvk.Engine
{


    struct Uniforms
    {
        public Matrix4x4 view;
        public Matrix4x4 proj;
    }

    internal class Main
    {
        private bool windowResized = false;
        private static Matrix4x4 test = MatrixModel(new Vector3(1, 1, 1), new Vector3(0, 0, 0), new Vector3(90f, 0, 0));
        private Vertex[] vertices =
        {
            new Vertex(new Vector3(275f, 275f,-1.0f), new Vector4(1.0f, 1.0f, 1.0f, 0.5f),
                new Vector2(0.0f, 1.0f),
                new Vector4(test.M11, test.M12, test.M13, test.M14),
                new Vector4(test.M21, test.M22, test.M23, test.M24),
                new Vector4(test.M31, test.M32, test.M33, test.M34),
                new Vector4(test.M41, test.M42, test.M43, test.M44)),

            new Vertex(new Vector3(275f, -275f, -1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector4(test.M11, test.M12, test.M13, test.M14),
                new Vector4(test.M21, test.M22, test.M23, test.M24),
                new Vector4(test.M31, test.M32, test.M33, test.M34),
                new Vector4(test.M41, test.M42, test.M43, test.M44)),

            new Vertex(new Vector3(-275f, -275f, -1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
                new Vector4(test.M11, test.M12, test.M13, test.M14),
                new Vector4(test.M21, test.M22, test.M23, test.M24),
                new Vector4(test.M31, test.M32, test.M33, test.M34),
                new Vector4(test.M41, test.M42, test.M43, test.M44)),

            new Vertex(new Vector3(-275f, 275f, -1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                new Vector2(0.0f, 0.0f),
                new Vector4(test.M11, test.M12, test.M13, test.M14),
                new Vector4(test.M21, test.M22, test.M23, test.M24),
                new Vector4(test.M31, test.M32, test.M33, test.M34),
                new Vector4(test.M41, test.M42, test.M43, test.M44)),
        };

        private VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("aTextureCoordinate", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("aPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("aColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlo", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlt", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlth", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("aModelMlf", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        ushort[] indices = new ushort[] { 0, 1, 2, 2, 3, 0 };
        List<string> shaderSource = new List<string>();

        private void readShaders(string[] source)
        {
            foreach (var shaders in source)
            {
                if (File.Exists(shaders))
                {
                    try
                    {
                        string sourceData = File.ReadAllText(shaders);
                        shaderSource.Add(sourceData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error read: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Error get file.");
                }
            }
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

        public unsafe void main()
        {
            readShaders(new string[] { ".\\Source\\Shaders\\VertexShader.glsl", ".\\Source\\Shaders\\FragmetShader.glsl" });
            WindowCreateInfo windowCI = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = "Veldrid Vulkan (Code name: Mikkarin Player alpha.0.0.1: MileStone: 1)"
            };

            Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);
            window.Resizable = true;

            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
                Debug = true
            };
            GraphicsDevice gd = VeldridStartup.CreateGraphicsDevice(window, options, GraphicsBackend.Vulkan);

            DeviceBuffer vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)(vertices.Length * Unsafe.SizeOf<Vertex>()), BufferUsage.VertexBuffer));

            DeviceBuffer indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));

            gd.UpdateBuffer(vertexBuffer, 0, vertices);
            gd.UpdateBuffer(indexBuffer, 0, indices);

            Uniforms uniforms = new Uniforms
            {
                view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0)),
                proj = Matrix4x4.CreateOrthographic((float)window.Width, (float)window.Height, 0.1f, 10.0f)
            };
            DeviceBuffer uniformBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<Uniforms>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            gd.UpdateBuffer(uniformBuffer, 0, ref uniforms);

            Texture depthTexture = gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)window.Width,
                (uint)window.Height,
                1,
                1,
                1,
                PixelFormat.R32_Float,
                TextureUsage.DepthStencil,
                TextureType.Texture2D));

            Framebuffer framebuffer = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthTexture));

            ResourceLayout resourceLayout = gd.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Uniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly    , ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SamplerT", ResourceKind.Sampler, ShaderStages.Fragment)
                ));

            TexturesBuffer texturesBuffer = new TexturesBuffer(gd);
            texturesBuffer.AddTexture(pathload: ".\\Source\\Textures\\def.png");

            ResourceSet resourceSet;
            resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                uniformBuffer,
                texturesBuffer.TexturesView.ToArray()[0], 
                texturesBuffer.SamplerConf));
           

            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(shaderSource[0]),
                "main");

            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(shaderSource[1]),
                "main");

            Shader[] shaders = gd.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            

            GraphicsPipelineDescription pipelineDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: ComparisonKind.LessEqual),
                new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new VertexLayoutDescription[] { vertexLayout }, shaders),
                new ResourceLayout[] { resourceLayout },
                gd.SwapchainFramebuffer.OutputDescription,
                ResourceBindingModel.Improved);

            Pipeline pipeline = gd.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);

            CommandList cl = gd.ResourceFactory.CreateCommandList();
            window.Resized += () => windowResized = true;
            

            while (window.Exists)
            {
                window.PumpEvents();

                if (!window.Exists) break;

                if (windowResized)
                {
                    gd.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                    windowResized = false;
                }

                uniforms.view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
                uniforms.proj = Matrix4x4.CreateOrthographic((float)window.Width, (float)window.Height, 0.1f, 10.0f);
                gd.UpdateBuffer(uniformBuffer, 0, ref uniforms);

                cl.Begin();
                cl.SetFramebuffer(framebuffer);
                cl.ClearDepthStencil(1.0f);
                cl.SetFramebuffer(gd.SwapchainFramebuffer);
                cl.ClearColorTarget(0, RgbaFloat.Black);

                cl.SetPipeline(pipeline);
                cl.SetGraphicsResourceSet(0, resourceSet);
                cl.SetVertexBuffer(0, vertexBuffer);
                cl.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);

                cl.DrawIndexed((uint)indices.Length);

                cl.End();

                gd.SubmitCommands(cl);
                gd.SwapBuffers();
            }

            gd.WaitForIdle();
            foreach (Shader shader in shaders)
            {
                shader.Dispose();
            }
            pipeline.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            uniformBuffer.Dispose();
            depthTexture.Dispose();
            cl.Dispose();
            gd.Dispose();
        }
    }
}
