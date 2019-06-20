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
    class ArrayTextureTest : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;
        VertexBuffer<VertexColorTexture> buffer;

        Texture2DArray texArr;

        public ArrayTextureTest() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, ColorFormat.Empty, 2), "3D FUCKSAAA LO PIBE", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(this.Context);
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

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

            texArr = new Texture2DArray(graphicsDevice, 512, 512, 4);
            texArr.SetData(GenTextureData(texArr.Width, texArr.Height, texArr.Depth), 0, 0, 0, 0, texArr.Width, texArr.Height, texArr.Depth);
            using(Bitmap b = new Bitmap("arrtest/t1.png"))
            {
                BitmapData bits = b.LockBits(new System.Drawing.Rectangle(0, 0, texArr.Width, texArr.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                texArr.SetData(bits.Scan0, 0, 0, 0, texArr.Width, texArr.Height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra);
                b.UnlockBits(bits);
            }
            using (Bitmap b = new Bitmap("arrtest/t2.png"))
            {
                BitmapData bits = b.LockBits(new System.Drawing.Rectangle(0, 0, texArr.Width, texArr.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                texArr.SetData(bits.Scan0, 0, 0, 1, texArr.Width, texArr.Height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra);
                b.UnlockBits(bits);
            }
            using (Bitmap b = new Bitmap("arrtest/t3.png"))
            {
                BitmapData bits = b.LockBits(new System.Drawing.Rectangle(0, 0, texArr.Width, texArr.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                texArr.SetData(bits.Scan0, 0, 0, 2, texArr.Width, texArr.Height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra);
                b.UnlockBits(bits);
            }
            texArr.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("arrtest/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("arrtest/fs.glsl"));
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            Matrix4 mat = Matrix4.CreateScale(1.5f);
            program.Uniforms["World"].SetValueMat4(ref mat);
            mat = Matrix4.Identity;
            program.Uniforms["View"].SetValueMat4(ref mat);
            mat = Matrix4.CreateScale(1f, -1f, 1f);
            program.Uniforms["Projection"].SetValueMat4(ref mat);
            program.Uniforms["samp"].SetValueTexture(texArr);

            buffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 255, 0, 255), new Vector2(1, 0)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0), new Color4b(0, 0, 255, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 0, 255, 255), new Vector2(1, 1)),
            }, BufferUsageHint.DynamicDraw);
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

            graphicsDevice.BindVertexArray(buffer.VertexArray);
            program.Uniforms["time"].SetValue1(time);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.SetViewport(0, 0, this.Width, this.Height);
        }

        protected override void OnUnload(EventArgs e)
        {

            graphicsDevice.Dispose();
        }

        private Color4b[] GenTextureData(int width, int height, int depth)
        {
            Color4b[] data = new Color4b[width * height * depth];
            int i = 0;
            for (int d = 0; d < depth; d++)
            {
                Color4b col = new Color4b[] { Color4b.Red, Color4b.Green, Color4b.Blue, Color4b.Magenta }[d];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        data[i++] = col;
            }
            return data;
        }
    }
}
