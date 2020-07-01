using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace TexturedTriangles
{
    // Renders a space image in the center of the window with a slow moving satellite.
    // On each side (left and right), three triangles are rendered showing the planets
    // in the texture (somewhat distorted). The triangles on the left don't apply any
    // color to the texture, but the triangles on the right do.

    class TexturedTriangles
    {
        Stopwatch stopwatch;
        IWindow window;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexColorTexture> quadBuffer;
        VertexBuffer<VertexColorTexture> trianglesBuffer;
        ShaderProgram shaderProgram;

        Texture2D background;
        Texture2D satellite;

        public TexturedTriangles()
        {
            window = CreateWindow();

            window.Load += OnWindowLoad;
            window.Update += OnWindowUpdate;
            window.Render += OnWindowRender;
            window.Resize += OnWindowResized;
            window.Closing += OnWindowClosing;
        }

        private IWindow CreateWindow()
        {
            GraphicsAPI graphicsApi = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(3, 3));
            VideoMode videoMode = new VideoMode(new System.Drawing.Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.Adaptive, 30, false, videoMode, 8);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void Run()
        {
            window.Run();
        }

        private void OnWindowLoad()
        {
            graphicsDevice = new GraphicsDevice(GL.GetApi(window));
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

            Console.WriteLine(string.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
            Console.WriteLine("GL Version String: " + graphicsDevice.GLVersion);
            Console.WriteLine("GL Vendor: " + graphicsDevice.GLVendor);
            Console.WriteLine("GL Renderer: " + graphicsDevice.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + graphicsDevice.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + graphicsDevice.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + graphicsDevice.MaxTextureSize);
            Console.WriteLine("GL MaxSamples: " + graphicsDevice.MaxSamples);

            Span<VertexColorTexture> quadsVertex = stackalloc VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0), Color4b.White, new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0), Color4b.White, new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0), Color4b.White, new Vector2(1, 1)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), Color4b.White, new Vector2(1, 0)),
            };

            quadBuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, quadsVertex, BufferUsageARB.StaticDraw);

            Span<VertexColorTexture> trianglesVertex = stackalloc VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.6f, -0.4f, 0), Color4b.White, new Vector2(0.02f, 0.35f)),
                new VertexColorTexture(new Vector3(-0.4f, -0.3f, 0), Color4b.White, new Vector2(0, 0.6f)),
                new VertexColorTexture(new Vector3(-0.6f, -0.2f, 0), Color4b.White, new Vector2(0.2f, 0.35f)),

                new VertexColorTexture(new Vector3(-0.6f, -0.1f, 0), Color4b.White, new Vector2(0.3f, 0.5f)),
                new VertexColorTexture(new Vector3(-0.4f, 0.0f, 0), Color4b.White, new Vector2(0.4f, 1)),
                new VertexColorTexture(new Vector3(-0.6f, 0.1f, 0), Color4b.White, new Vector2(0.6f, 0.55f)),

                new VertexColorTexture(new Vector3(-0.6f, 0.2f, 0), Color4b.White, new Vector2(0.82f, 0.2f)),
                new VertexColorTexture(new Vector3(-0.4f, 0.3f, 0), Color4b.White, new Vector2(0.98f, 0.09f)),
                new VertexColorTexture(new Vector3(-0.6f, 0.4f, 0), Color4b.White, new Vector2(0.89f, 0.26f)),

                new VertexColorTexture(new Vector3(0.6f, -0.4f, 0), Color4b.Lime, new Vector2(0.02f, 0.35f)),
                new VertexColorTexture(new Vector3(0.4f, -0.3f, 0), Color4b.Lime, new Vector2(0, 0.6f)),
                new VertexColorTexture(new Vector3(0.6f, -0.2f, 0), Color4b.Lime, new Vector2(0.2f, 0.35f)),

                new VertexColorTexture(new Vector3(0.6f, -0.1f, 0), Color4b.Red, new Vector2(0.3f, 0.5f)),
                new VertexColorTexture(new Vector3(0.4f, 0.0f, 0), Color4b.Lime, new Vector2(0.4f, 1)),
                new VertexColorTexture(new Vector3(0.6f, 0.1f, 0), Color4b.Blue, new Vector2(0.6f, 0.55f)),

                new VertexColorTexture(new Vector3(0.6f, 0.2f, 0), Color4b.Red, new Vector2(0.82f, 0.2f)),
                new VertexColorTexture(new Vector3(0.4f, 0.3f, 0), Color4b.Red, new Vector2(0.98f, 0.09f)),
                new VertexColorTexture(new Vector3(0.6f, 0.4f, 0), Color4b.Red, new Vector2(0.89f, 0.26f)),
            };

            trianglesBuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, trianglesVertex, BufferUsageARB.StaticDraw);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            background = Texture2DExtensions.FromFile(graphicsDevice, "texture.png");
            background.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            satellite = Texture2DExtensions.FromFile(graphicsDevice, "satellite.png");
            satellite.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthTestingEnabled = false;

            stopwatch = Stopwatch.StartNew();

            OnWindowResized(window.Size);
        }

        private void OnWindowUpdate(double dtSeconds)
        {
            GLEnum c;
            while ((c = graphicsDevice.GL.GetError()) != GLEnum.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        private void OnWindowRender(double dtSeconds)
        {
            if (window.IsClosing)
                return;

            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.ShaderProgram = shaderProgram;

            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.Identity);
            shaderProgram.Uniforms["samp"].SetValueTexture(background);
            graphicsDevice.VertexArray = trianglesBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, trianglesBuffer.StorageLength);

            graphicsDevice.VertexArray = quadBuffer;

            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.CreateScale(0.6f));
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            float time = (float)stopwatch.Elapsed.TotalSeconds * MathF.PI * 0.1f;
            Matrix4x4 satelliteMatrix = Matrix4x4.CreateScale(satellite.Width / (float)satellite.Height * 0.15f, 0.15f, 1f);
            satelliteMatrix *= Matrix4x4.CreateRotationZ((float)Math.Sin(time) * 0.2f);
            satelliteMatrix *= Matrix4x4.CreateTranslation(0f, 0.2f, 0f);
            satelliteMatrix *= Matrix4x4.CreateRotationZ((float)Math.Cos(time) * 0.33f);
            shaderProgram.Uniforms["World"].SetValueMat4(satelliteMatrix);
            shaderProgram.Uniforms["samp"].SetValueTexture(satellite);
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(window.Size.Width / (float)window.Size.Height, 1f, 0.01f, 10f));
        }

        private void OnWindowClosing()
        {
            trianglesBuffer.Dispose();
            quadBuffer.Dispose();
            shaderProgram.Dispose();
            background.Dispose();
            satellite.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
