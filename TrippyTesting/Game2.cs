using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using TrippyGL;

namespace TrippyTesting
{
    class Game2 : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        Random r = new Random();
        float time;

        ShaderProgram program;
        ShaderUniform worldUniform, viewUniform, projUniform, sampUniform, timeUniform;

        VertexBuffer<VertexColorTexture> vertexBuffer;

        Texture2D yarn, jeru, texture, invernadero;

        public Game2() : base(1280, 720, new GraphicsMode(new ColorFormat(8,8,8,8), 0, 0, 8, ColorFormat.Empty, 2), "T R I P P Y", GameWindowFlags.Default, DisplayDevice.Default, 4, 4, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            TrippyLib.Init();
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            VertexColorTexture[] vboData = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0), new Color4b(0, 255, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 0, 255, 255), new Vector2(1, 1)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 0)),
            };
            vertexBuffer = new VertexBuffer<VertexColorTexture>(vboData.Length, vboData, BufferUsageHint.DynamicDraw);

            program = new ShaderProgram();
            program.AddVertexShader(File.ReadAllText("data2/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("data2/fs.glsl"));
            program.SpecifyVertexAttribs(vertexBuffer.VertexArray.AttribSources, new string[] {"vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            worldUniform = program.Uniforms["World"];
            viewUniform = program.Uniforms["View"];
            projUniform = program.Uniforms["Projection"];
            sampUniform = program.Uniforms["samp"];
            timeUniform = program.Uniforms["time"];

            Matrix4 ide = Matrix4.Identity;
            worldUniform.SetValue(ref ide);
            viewUniform.SetValue(ref ide);
            projUniform.SetValue(ref ide);

            yarn = new Texture2D("data/YARN.png");
            jeru = new Texture2D("data/jeru.png");
            texture = new Texture2D("data/texture.png");
            invernadero = new Texture2D("data/invernadero.png");

            Texture2D[] hehe = new Texture2D[]
            {
                yarn, jeru, invernadero, texture, jeru
            };
            sampUniform.SetValue(hehe, 0, 0, hehe.Length);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            time = (float)stopwatch.Elapsed.TotalSeconds;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            BlendMode.AlphaBlend.Apply();
            vertexBuffer.EnsureArrayBound();
            program.EnsureInUse();
            program.Uniforms.EnsureSamplerUniformsSet();
            timeUniform.SetValue(time);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            SwapBuffers();

            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            vertexBuffer.Dispose();
            yarn.Dispose();
            jeru.Dispose();
            texture.Dispose();
            invernadero.Dispose();

            TrippyLib.Quit();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }
    }
}
