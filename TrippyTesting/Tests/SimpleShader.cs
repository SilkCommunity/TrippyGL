using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.Numerics;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class SimpleShader
    {
        System.Diagnostics.Stopwatch stopwatch;

        IWindow window;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexNormal> dragonBuffer;
        SimpleShaderProgram dragonProgram;
        SimpleShaderProgram dragonProgramTwo;

        Texture2D stallTexture;
        VertexBuffer<VertexNormalTexture> stallBuffer;
        SimpleShaderProgram stallProgram;
        SimpleShaderProgram stallProgramTwo;

        public SimpleShader()
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
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 24);
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

            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            VertexNormal[] dragonModel = OBJLoader.FromFile<VertexNormal>("objs/dragon.obj");
            dragonBuffer = new VertexBuffer<VertexNormal>(graphicsDevice, dragonModel, BufferUsageARB.StaticDraw);

            SimpleShaderProgramBuilder programBuilder = new SimpleShaderProgramBuilder()
            {
                DirectionalLights = 1,
                PositionalLights = 1
            };
            programBuilder.SpecifyVertexAttribs<VertexNormal>();
            dragonProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            dragonProgram.View = Matrix4x4.CreateLookAt(new Vector3(0, 6, -9), new Vector3(0, 4, 0), Vector3.UnitY);
            dragonProgram.PositionalLights[0].SpecularColor = new Vector3(0, 0, 1);
            dragonProgram.PositionalLights[0].DiffuseColor = new Vector3(0, 0, 1);
            dragonProgram.DirectionalLights[0].DiffuseColor = new Vector3(1, 0, 0);
            dragonProgram.DirectionalLights[0].SpecularColor = new Vector3(0, 1, 0);
            dragonProgram.SpecularPower = 15f;
            dragonProgram.Reflectivity = 1;
            dragonProgram.AmbientLightColor = new Vector3(0, 0.17f, 0);

            programBuilder = new SimpleShaderProgramBuilder();
            programBuilder.SpecifyVertexAttribs<VertexNormal>();
            dragonProgramTwo = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            dragonProgramTwo.View = Matrix4x4.CreateLookAt(new Vector3(0, 6, -9), new Vector3(0, 4, 0), Vector3.UnitY);
            dragonProgramTwo.Color = new Vector4(1, 0, 0, 1);

            stallTexture = Texture2DExtensions.FromFile(graphicsDevice, "objs/stallTexture.png");

            VertexNormalTexture[] stallModel = OBJLoader.FromFile<VertexNormalTexture>("objs/stall.obj");
            stallBuffer = new VertexBuffer<VertexNormalTexture>(graphicsDevice, stallModel, BufferUsageARB.StaticDraw);

            programBuilder = new SimpleShaderProgramBuilder()
            {
                TextureEnabled = true,
                DirectionalLights = 1
            };
            programBuilder.SpecifyVertexAttribs<VertexNormalTexture>();
            stallProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            stallProgram.View = Matrix4x4.CreateLookAt(new Vector3(0, 4, -6), new Vector3(0, 1.6f, 0), Vector3.UnitY);
            stallProgram.SpecularPower = 15f;
            stallProgram.Reflectivity = 1;
            stallProgram.Texture = stallTexture;
            stallProgram.DirectionalLights[0].SpecularColor = new Vector3(0, 0, 1);
            stallProgram.AmbientLightColor = new Vector3(0, 0.17f, 0);

            programBuilder = new SimpleShaderProgramBuilder()
            {
                TextureEnabled = true
            };
            programBuilder.SpecifyVertexAttribs<VertexNormalTexture>();
            stallProgramTwo = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            stallProgramTwo.Texture = stallTexture;
            stallProgramTwo.View = Matrix4x4.CreateLookAt(new Vector3(0, 4, -6), new Vector3(0, 1.6f, 0), Vector3.UnitY);

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthState = DepthTestingState.Default;

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

            float time = (float)stopwatch.Elapsed.TotalSeconds;

            uint wd2 = (uint)window.Size.Width / 2;
            uint hd2 = (uint)window.Size.Height / 2;

            dragonProgram.PositionalLights[0].Position = new Vector3(0, 8 * (time % 3) - 12 + 4, 0);

            graphicsDevice.ScissorTestEnabled = false;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            graphicsDevice.ScissorTestEnabled = true;

            graphicsDevice.VertexArray = dragonBuffer;

            graphicsDevice.SetViewport(0, (int)hd2, wd2, hd2);
            graphicsDevice.SetScissorRectangle(0, (int)hd2, wd2, hd2);
            graphicsDevice.ClearColor = new Vector4(0.2f, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.ShaderProgram = dragonProgram;
            dragonProgram.DirectionalLights[0].Direction = new Vector3(MathF.Cos(time), -1, MathF.Sin(time));
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, dragonBuffer.StorageLength);

            graphicsDevice.SetViewport((int)wd2, (int)hd2, wd2, hd2);
            graphicsDevice.SetScissorRectangle((int)wd2, (int)hd2, wd2, hd2);
            graphicsDevice.ClearColor = new Vector4(0, 0.2f, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.ShaderProgram = dragonProgramTwo;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, dragonBuffer.StorageLength);

            graphicsDevice.VertexArray = stallBuffer;

            graphicsDevice.SetViewport(0, 0, wd2, hd2);
            graphicsDevice.SetScissorRectangle(0, 0, wd2, hd2);
            graphicsDevice.ClearColor = new Vector4(0, 0, 0.2f, 1f);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.ShaderProgram = stallProgram;
            stallProgram.DirectionalLights[0].Direction = new Vector3(MathF.Cos(time), -1, MathF.Sin(time));
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, stallBuffer.StorageLength);

            graphicsDevice.SetViewport((int)wd2, 0, wd2, hd2);
            graphicsDevice.SetScissorRectangle((int)wd2, 0, wd2, hd2);
            graphicsDevice.ShaderProgram = stallProgramTwo;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, stallBuffer.StorageLength);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, window.Size.Width / (float)window.Size.Height, 0.1f, 50f);
            dragonProgram.Projection = proj;
            dragonProgramTwo.Projection = proj;
            stallProgram.Projection = proj;
            stallProgramTwo.Projection = proj;
        }

        private void OnWindowClosing()
        {
            dragonBuffer.Dispose();
            dragonProgram.Dispose();
            dragonProgramTwo.Dispose();
            stallTexture.Dispose();
            stallBuffer.Dispose();
            stallProgram.Dispose();
            stallProgramTwo.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
