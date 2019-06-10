using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using TrippyGL;
using System.Runtime.InteropServices;

namespace TrippyTesting
{
    class TransformGame : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        Random r = new Random();

        GraphicsDevice graphicsDevice;

        VertexDataBufferObject<ParticleVertex> bufferRead, bufferWrite;
        VertexArray vaoRead, vaoWrite;

        int transProgram, timeLoc, deltaTimeLoc;
        int drawProgram, worldLoc, viewLoc, projLoc;

        float time;
        float deltaTime;

        public TransformGame() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "T R A N S F O R M S", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
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

            #region LoadVertex
            ParticleVertex[] bufferData = new ParticleVertex[5000];
            for (int i = 0; i < bufferData.Length; i++)
                bufferData[i] = new ParticleVertex(new Vector3(RandomFloat(0, 1), RandomFloat(0, 1), 0), RandomFullAlphaColor());

            bufferRead = new VertexDataBufferObject<ParticleVertex>(graphicsDevice, bufferData.Length, bufferData, BufferUsageHint.DynamicDraw);
            bufferWrite = new VertexDataBufferObject<ParticleVertex>(graphicsDevice, bufferData.Length, BufferUsageHint.DynamicDraw);
            vaoRead = new VertexArray(graphicsDevice, new VertexAttribSource[] {
                new VertexAttribSource(bufferRead, ActiveAttribType.FloatVec3),
                new VertexAttribSource(bufferRead, ActiveAttribType.FloatVec4)
            });
            vaoWrite = new VertexArray(graphicsDevice, new VertexAttribSource[] {
                new VertexAttribSource(bufferWrite, ActiveAttribType.FloatVec3),
                new VertexAttribSource(bufferWrite, ActiveAttribType.FloatVec4)
            });

            #endregion

            #region CreateTransProgram
            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, File.ReadAllText("transdata/ptcvs.glsl"));
            GL.CompileShader(vs);

            transProgram = GL.CreateProgram();
            GL.AttachShader(transProgram, vs);
            GL.BindAttribLocation(transProgram, 0, "vPosition");
            GL.BindAttribLocation(transProgram, 1, "vColor");
            GL.TransformFeedbackVaryings(transProgram, 2, new string[] { "outPosition", "outColor" }, TransformFeedbackMode.InterleavedAttribs);
            GL.LinkProgram(transProgram);
            GL.DetachShader(transProgram, vs);
            GL.DeleteShader(vs);

            timeLoc = GL.GetUniformLocation(transProgram, "time");
            deltaTimeLoc = GL.GetUniformLocation(transProgram, "deltaTime");

            Console.WriteLine("TransProgram Log: \n" + GL.GetProgramInfoLog(transProgram) + "\n[END PROGRAM LOG]");
            #endregion

            #region CreateDrawProgram
            vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, File.ReadAllText("transdata/drawvs.glsl"));
            GL.CompileShader(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, File.ReadAllText("transdata/drawfs.glsl"));
            GL.CompileShader(fs);

            int gs = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(gs, File.ReadAllText("transdata/drawgs.glsl"));
            GL.CompileShader(gs);

            drawProgram = GL.CreateProgram();
            GL.AttachShader(drawProgram, vs);
            GL.AttachShader(drawProgram, fs);
            GL.AttachShader(drawProgram, gs);
            GL.BindAttribLocation(drawProgram, 0, "vPosition");
            GL.BindAttribLocation(drawProgram, 1, "vColor");
            GL.LinkProgram(drawProgram);
            GL.DetachShader(drawProgram, vs);
            GL.DetachShader(drawProgram, fs);
            GL.DetachShader(drawProgram, gs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
            GL.DeleteShader(gs);

            worldLoc = GL.GetUniformLocation(drawProgram, "World");
            viewLoc = GL.GetUniformLocation(drawProgram, "View");
            projLoc = GL.GetUniformLocation(drawProgram, "Projection");
            GL.UseProgram(drawProgram);
            Matrix4 mat = Matrix4.Identity;
            GL.UniformMatrix4(worldLoc, false, ref mat);
            GL.UniformMatrix4(viewLoc, false, ref mat);
            GL.UniformMatrix4(projLoc, false, ref mat);

            Console.WriteLine("DrawProgram Log: \n" + GL.GetProgramInfoLog(drawProgram) + "\n[END OF PROGRAM LOG]");
            #endregion
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float prevtime = time;
            time = (float)stopwatch.Elapsed.TotalSeconds;
            deltaTime = time - prevtime;

            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Enable(EnableCap.RasterizerDiscard);
            graphicsDevice.BindVertexArray(vaoRead);
            GL.UseProgram(transProgram);
            if (timeLoc != -1)
                GL.Uniform1(timeLoc, time);
            if (deltaTimeLoc != -1)
                GL.Uniform1(deltaTimeLoc, deltaTime);
            GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, 0, bufferWrite.Handle);
            GL.BeginTransformFeedback(TransformFeedbackPrimitiveType.Points);

            GL.DrawArrays(PrimitiveType.Points, 0, bufferRead.StorageLength);

            GL.EndTransformFeedback();

            GL.Disable(EnableCap.RasterizerDiscard);
            BlendState.AlphaBlend.Apply();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, this.Width, this.Height);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(drawProgram);

            graphicsDevice.BindVertexArray(vaoWrite);
            GL.DrawArrays(PrimitiveType.Points, 0, bufferRead.StorageLength);

            VertexDataBufferObject<ParticleVertex> tmpvbo = bufferRead;
            bufferRead = bufferWrite;
            bufferWrite = tmpvbo;
            VertexArray tmpvao = vaoRead;
            vaoRead = vaoWrite;
            vaoWrite = tmpvao;

            SwapBuffers();
            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteProgram(transProgram);
            bufferRead.Dispose();
            bufferWrite.Dispose();
            vaoRead.Dispose();
            vaoWrite.Dispose();

            graphicsDevice.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            Matrix4 mat = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
            GL.UseProgram(drawProgram);
            GL.UniformMatrix4(projLoc, false, ref mat);
        }

        float RandomFloat(float min, float max)
        {
            return min + (float)r.NextDouble() * (max - min);
        }

        Color4 RandomFullAlphaColor()
        {
            return new Color4((byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256), 255);
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    struct ParticleVertex
    {
        public Vector3 Position;
        public Color4 Color;

        public ParticleVertex(Vector3 position, Color4 color)
        {
            this.Position = position;
            this.Color = color;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X, ", ", Position.Y, ", ", Position.Z, ") (", Color.R, ", ", Color.G, ", ", Color.B, ", ", Color.A, ")");
        }
    }
}
