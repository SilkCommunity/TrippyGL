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
    class FramebufferTest2 : GameWindow
    {
        Random r = new Random();
        System.Diagnostics.Stopwatch stopwatch;
        float time;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;
        VertexArray array;
        VertexDataBufferObject<VertexColor> buffer;

        Framebuffer2D framebuffer;

        ShaderProgram fuckprogram;
        VertexBuffer<VertexTexture> fuckbuffer;

        float mouseX, mouseY;

        public FramebufferTest2() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Debug)
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(Program.OnDebugMessage, IntPtr.Zero);
            
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
            GL.Enable(EnableCap.Multisample);
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            time = 0;

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("dataa3/vs3d.glsl"));
            program.AddFragmentShader(File.ReadAllText("dataa3/fs3d.glsl"));
            program.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition" ,"vColor" });
            program.LinkProgram();

            buffer = new VertexDataBufferObject<VertexColor>(graphicsDevice, new VertexColor[]
            {
                new VertexColor(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(-0.5f, 0.5f, 0), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 255, 0, 255))
            }, BufferUsageHint.DynamicDraw);

            array = VertexArray.CreateSingleBuffer<VertexColor>(graphicsDevice, buffer);

            fuckprogram = new ShaderProgram(graphicsDevice);
            fuckprogram.AddVertexShader("#version 400\r\nin vec3 vP; in vec2 vT; out vec2 fT; void main() { gl_Position = vec4(vP, 1.0); fT = vT; }");
            fuckprogram.AddFragmentShader("#version 400\r\nuniform sampler2DMS samp; uniform vec2 res; uniform int s; in vec2 fT; out vec4 FragColor; void main() { for(int i=0; i<s; i++) { FragColor += texelFetch(samp, ivec2(int(fT.x * res.x), int(fT.y * res.y)), i); } FragColor /= float(s); }");
            fuckprogram.SpecifyVertexAttribs<VertexTexture>(new string[] { "vP", "vT" });
            fuckprogram.LinkProgram();
            fuckbuffer = new VertexBuffer<VertexTexture>(graphicsDevice, new VertexTexture[]
            {
                new VertexTexture(new Vector3(-1, -1, 0), new Vector2(0, 0)),
                new VertexTexture(new Vector3(1, -1, 0), new Vector2(1, 0)),
                new VertexTexture(new Vector3(-1, 1, 0), new Vector2(0, 1)),
                new VertexTexture(new Vector3(1, 1, 0), new Vector2(1, 1)),
            }, BufferUsageHint.StaticDraw);

            framebuffer = new Framebuffer2D(graphicsDevice, this.Width / 2, this.Height / 2, DepthStencilFormat.None, TextureImageFormat.Color4b, 8);
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
            Matrix4 mat = Matrix4.Identity;
            GL.ClearColor(0f, 0f, 0f, 1f);

            graphicsDevice.BindFramebuffer(framebuffer);
            graphicsDevice.SetViewport(0, 0, framebuffer.Width, framebuffer.Height);

            graphicsDevice.BindVertexArray(array);

            float tx = (mouseX / (float)this.Width) * 2 - 1;
            float ty = (1f - mouseY / (float)this.Height) * 2 - 1;
            mat = Matrix4.CreateRotationZ(time * 3.14f) * Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(tx, ty, 0);
            program.Uniforms["World"].SetValueMat4(ref mat);
            mat = Matrix4.Identity;
            program.Uniforms["View"].SetValueMat4(ref mat);
            program.Uniforms["Projection"].SetValueMat4(ref mat);
            program.EnsurePreDrawStates();

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            graphicsDevice.BindFramebuffer(null);
            graphicsDevice.SetViewport(0, 0, this.Width, this.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            fuckprogram.Uniforms["samp"].SetValueTexture(framebuffer.Texture);
            fuckprogram.Uniforms["res"].SetValue2(new Vector2(framebuffer.Width, framebuffer.Height));
            fuckprogram.Uniforms["s"].SetValue1(framebuffer.Samples);
            fuckprogram.EnsurePreDrawStates();
            graphicsDevice.UseShaderProgram(fuckprogram);
            graphicsDevice.BindVertexArray(fuckbuffer.VertexArray);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            const int jaja = 8;
            framebuffer.ReacreateFramebuffer(this.Width / jaja, this.Height / jaja);
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            array.Dispose();
            buffer.Dispose();
            framebuffer.Dispose();

            graphicsDevice.Dispose();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Middle)
            {
                using (Framebuffer2D fb = new Framebuffer2D(graphicsDevice, this.Width, this.Height, DepthStencilFormat.None))
                {
                    graphicsDevice.BindFramebufferRead(framebuffer);
                    graphicsDevice.BindFramebufferDraw(fb);

                    graphicsDevice.SetViewport(0, 0, fb.Width, fb.Height);
                    GL.ClearColor(0f, 0f, 0f, 0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    fuckprogram.Uniforms["samp"].SetValueTexture(framebuffer.Texture);
                    fuckprogram.Uniforms["res"].SetValue2(new Vector2(framebuffer.Width, framebuffer.Height));
                    fuckprogram.Uniforms["s"].SetValue1(framebuffer.Samples);
                    fuckprogram.EnsurePreDrawStates();
                    graphicsDevice.UseShaderProgram(fuckprogram);
                    graphicsDevice.BindVertexArray(fuckbuffer.VertexArray);

                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

                    // Blitting with multisampled framebuffers only works if the src and dst rectangles have the same size
                    //graphicsDevice.BlitFramebuffer(framebuffer, fb, 0, 0, framebuffer.Width, framebuffer.Height, 0, 0, fb.Width, fb.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
                    
                    fb.SaveAsImage(String.Concat("save", time.ToString(), ".png"), SaveImageFormat.Png);
                }
            }
        }
    }
}
