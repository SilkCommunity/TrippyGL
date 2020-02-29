using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.IO;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class DoSumShit : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;

        BufferObject buffer;
        VertexDataBufferSubset<VertexColorTexture> bufferSubset;
        VertexArray vertexArray;

        Texture2D texture;

        public DoSumShit() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(Context);
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

            Console.WriteLine(string.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
            Console.WriteLine("GL Version String: " + graphicsDevice.GLVersion);
            Console.WriteLine("GL Vendor: " + graphicsDevice.GLVendor);
            Console.WriteLine("GL Renderer: " + graphicsDevice.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + graphicsDevice.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + graphicsDevice.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + graphicsDevice.MaxTextureSize);
            Console.WriteLine("GL MaxSamples:" + graphicsDevice.MaxSamples);
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            time = 0;

            VertexColorTexture[] vertices = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(0, 0, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(1, 0, 0), new Color4b(0, 255, 0, 255), new Vector2(1, 1)),
                new VertexColorTexture(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(1, 1, 0), new Color4b(255, 255, 0, 255), new Vector2(1, 0)),
            };

            buffer = new BufferObject(graphicsDevice, vertices.Length * VertexColorTexture.SizeInBytes, BufferUsageHint.StaticDraw);
            bufferSubset = new VertexDataBufferSubset<VertexColorTexture>(buffer, vertices);
            vertexArray = VertexArray.CreateSingleBuffer<VertexColorTexture>(graphicsDevice, bufferSubset);

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("sumshit/simple_vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("sumshit/simple_fs.glsl"));
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            texture = new Texture2D(graphicsDevice, "data4/jeru.png", true);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphicsDevice.Framebuffer = null;
            graphicsDevice.SetViewport(0, 0, Width, Height);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            graphicsDevice.VertexArray = vertexArray;
            graphicsDevice.ShaderProgram = program;

            Matrix4 mat = Matrix4.Identity;
            program.Uniforms["World"].SetValueMat4(ref mat);
            program.Uniforms["View"].SetValueMat4(ref mat);
            program.Uniforms["Projection"].SetValueMat4(ref mat);
            program.Uniforms["tex"].SetValueTexture(texture);
            
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthState = DepthTestingState.None;
        }

        protected override void OnUnload(EventArgs e)
        {
            graphicsDevice.DisposeAllResources();
            graphicsDevice.Dispose();
        }
    }
}
