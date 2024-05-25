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
        private VertexMeneg vertexMeneg = new VertexMeneg();
        private Vertex[] vertices;

        ushort[] indices ;
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

            this.vertexMeneg.NewVertexSquare(new Square(
                Width: 64,
                Height: 64,
                BagroundColor: new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                Z_Index: 2,
                Scale: new Vector3(1, 1, 1)
            ));

            this.vertexMeneg.NewVertexSquare(new Square(
                Width: 128,
                Height: 32,
                BagroundColor: new Vector4(1.0f, 1.0f, 0.0f, 0.5f),
                Z_Index: 1,
                Scale: new Vector3(1, 1, 1)
            ));
            this.vertices = this.vertexMeneg.vertex.SelectMany(array => array).ToArray();
            this.indices = this.vertexMeneg.indices.SelectMany(array => array).ToArray();

            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
                SyncToVerticalBlank = true,
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
                new ShaderSetDescription(new VertexLayoutDescription[] { this.vertexMeneg.GetLayoutVertex() }, shaders),
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
