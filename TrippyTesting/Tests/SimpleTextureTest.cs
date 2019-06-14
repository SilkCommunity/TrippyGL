using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class SimpleTextureTest : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        float time;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;
        VertexBuffer<VertexColorTexture> buffer;
        Texture2D tex1, tex2;

        public SimpleTextureTest() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "Simple Triangle", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(this.Context);

            Console.WriteLine(String.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
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

            VertexColorTexture[] data = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0f), new Color4b(255, 0, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0f), new Color4b(0, 255, 0, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0f), new Color4b(0, 0, 255, 255), new Vector2(1, 0)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0f), new Color4b(255, 255, 255, 255), new Vector2(1, 1)),
            };

            buffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, data.Length, data, BufferUsageHint.DynamicDraw);

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader("#version 400\r\nuniform mat4 w, v, p; in vec3 vP; in vec4 vC; in vec2 vT; out vec4 fC; out vec2 fT; void main() { fC = vC; fT = vT; gl_Position = p * v * w * vec4(vP, 1.0); }");
            program.AddFragmentShader("#version 400\r\nuniform sampler2D tex; uniform float t; in vec4 fC; in vec2 fT; out vec4 FragColor; void main() { FragColor = (fC*fract(t) + 1.0-fract(t)) * texture(tex, fT); }");
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] {"vP", "vC", "vT" });
            program.LinkProgram();
            Console.WriteLine("Program info log: \n" + GL.GetProgramInfoLog(program.Handle) + "\n[END OF LOG]");

            tex1 = new Texture2D(graphicsDevice, "data/jeru.png");
            tex2 = new Texture2D(graphicsDevice, "data/texture.png");

            tex1.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            tex2.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
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
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            buffer.EnsureArrayBound();
            graphicsDevice.UseShaderProgram(program);
            program.Uniforms["t"].SetValue1(time);
            program.Uniforms["tex"].SetValueTexture(tex1);

            Matrix4 mat = Matrix4.CreateScale(0.4f) * Matrix4.CreateTranslation(0.25f, 0.5f, 0f);
            program.Uniforms["w"].SetValueMat4(ref mat);

            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            mat = Matrix4.CreateScale(0.4f) * Matrix4.CreateTranslation(0.75f, 0.5f, 0f);
            program.Uniforms["w"].SetValueMat4(ref mat);
            program.Uniforms["tex"].SetValueTexture(tex2);

            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();

            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            buffer.Dispose();
            tex1.Dispose();
            tex2.Dispose();

            graphicsDevice.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);

            Matrix4 mat = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
            program.Uniforms["p"].SetValueMat4(ref mat);

            mat = Matrix4.Identity;
            program.Uniforms["v"].SetValueMat4(ref mat);
        }
    }
}
