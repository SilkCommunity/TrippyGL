﻿using System;
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
    class Game : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        Random r = new Random();

        VertexDataBufferObject<Vector5> posTexBuffer;
        VertexDataBufferObject<Color4b> colBuffer;

        IndexBufferObject indexBuffer;

        VertexArray vertexArray;

        int program, worldUniform, viewUniform, projUniform, texUniform;

        Texture2D texture, fondo, invernadero, jeru, plant, yarn;

        float time;

        public Game() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 8, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 4, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            TrippyLib.Init();
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            BlendMode.AlphaBlend.Apply();

            #region LoadVBO

            posTexBuffer = new VertexDataBufferObject<Vector5>(4, new Vector5[]
            {
                new Vector5(-0.5f, -0.5f, 0, 0, 0),
                new Vector5(0.5f, -0.5f, 0, 1, 0),
                new Vector5(-0.5f, 0.5f, 0, 0, 1),
                new Vector5(0.5f, 0.5f, 0, 1, 1)
            }, BufferUsageHint.DynamicDraw);
            Console.WriteLine(GL.GetError());
            colBuffer = new VertexDataBufferObject<Color4b>(4, new Color4b[]
            {
                new Color4b(255, 255, 255, 255),
                new Color4b(255, 255, 255, 255),
                new Color4b(255, 255, 255, 255),
                new Color4b(255, 255, 255, 255)
            }, BufferUsageHint.DynamicDraw);

            vertexArray = new VertexArray(new VertexAttribSource[]
            {
                new VertexAttribSource(posTexBuffer, 3, false, VertexAttribPointerType.Float),
                new VertexAttribSource(colBuffer, 4, true, VertexAttribPointerType.UnsignedByte),
                new VertexAttribSource(posTexBuffer, 2, false, VertexAttribPointerType.Float),
            });

            indexBuffer = new IndexBufferObject(4, 0, new byte[]
            {
                3, 1, 2, 0
            }, BufferUsageHint.DynamicDraw);

            #endregion

            #region LoadShaderProgram
            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, File.ReadAllText("data/vs.glsl"));
            GL.CompileShader(vs);
            //Console.WriteLine("VS: " + GL.GetShaderInfoLog(vs));

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, File.ReadAllText("data/fs.glsl"));
            GL.CompileShader(fs);
            //Console.WriteLine("FS: " + GL.GetShaderInfoLog(fs));

            program = GL.CreateProgram();
            GL.AttachShader(program, vs);
            GL.AttachShader(program, fs);
            GL.BindAttribLocation(program, 0, "vPosition");
            GL.BindAttribLocation(program, 1, "vColor");
            GL.BindAttribLocation(program, 2, "vTexCoords");
            GL.LinkProgram(program);
            GL.DetachShader(program, vs);
            GL.DetachShader(program, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            worldUniform = GL.GetUniformLocation(program, "World");
            viewUniform = GL.GetUniformLocation(program, "View");
            projUniform = GL.GetUniformLocation(program, "Projection");
            texUniform = GL.GetUniformLocation(program, "texture");
            GL.UseProgram(program);
            Matrix4 i = Matrix4.Identity;
            GL.UniformMatrix4(worldUniform, false, ref i);
            GL.UniformMatrix4(viewUniform, false, ref i);
            GL.UniformMatrix4(projUniform, false, ref i);

            Console.WriteLine("Program: \n" + GL.GetProgramInfoLog(program));
            Console.WriteLine("[end program log]");
            #endregion

            #region LoadTexture
            texture = new Texture2D("data/texture.png");
            fondo = new Texture2D("data/fondo.png");
            invernadero = new Texture2D("data/invernadero.png");
            jeru = new Texture2D("data/jeru.png");
            plant = new Texture2D("data/plant.png");
            yarn = new Texture2D("data/YARN.png");
            #endregion
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
            TrippyLib.ResetGLBindStates();

            posTexBuffer.SetData(1, 0, 2, new Vector5[]
            {
                new Vector5(0.5f+wave(0.05f, time, 1.7f, 0f), -0.5f+wave(0.05f, time, 1.81f, 0f), 0, 1, 0),
                new Vector5(-0.5f+wave(0.05f, time, 1.44f, 0f), 0.5f+wave(0.05f, time, 1.47f, 0f), 0, 0, 1),
            });

            byte intensity = (byte)((wave(0.5f, time, 3f, 0f) + 0.5f) * 255);
            colBuffer.SetData(2, 1, 2, new Color4b[]
            {
                new Color4b(intensity, intensity, intensity, 255),
                new Color4b(intensity, intensity, intensity, 255),
                new Color4b(intensity, intensity, intensity, 255)
            });

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, this.Width, this.Height);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Color4b[] xdxd = new Color4b[]
            {
                randomColor(), randomColor(), randomColor(), randomColor(),
                randomColor(), randomColor(), randomColor(), randomColor(),
                randomColor(), randomColor(), randomColor(), randomColor(),
                randomColor(), randomColor(), randomColor(), randomColor()
            };
            fondo.SetData(xdxd, r.Next(fondo.Width - 4), r.Next(fondo.Height - 4), 4, 4);
            jeru.SetData(xdxd, r.Next(jeru.Width - 4), r.Next(jeru.Height - 4), 4, 4);

            drawTexture(fondo, new Vector2(fondo.Width / 2f, fondo.Height / 2f), new Vector2(1f), 0f);
            drawTexture(texture, new Vector2(500, 300), new Vector2(0.35f), time * 0.1f);
            drawTexture(invernadero, new Vector2(700, 200), new Vector2(0.35f), (float)Math.Sin(time * 6.28f) * 0.3f);
            for (int i = 0; i < 5; i++)
                drawTexture(plant, new Vector2(i * 100 + 200, 400), new Vector2(0.5f), i * 0.1f);
            drawTexture(jeru, new Vector2(fondo.Width / 2f, fondo.Height / 2f), new Vector2(1.2f), time * 0.3f);
            drawTexture(jeru, new Vector2(((float)Math.Sin(time * 0.8f + 8) * 0.5f + 0.5f) * fondo.Width, ((float)Math.Cos(time * 1.2f + 3) * 0.5f + 0.5f) * fondo.Height), new Vector2(0.3f), (float)Math.Sin(time * 3.1416f) * 0.5f);

            SwapBuffers();

            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteProgram(program);
            texture.Dispose();
            fondo.Dispose();
            invernadero.Dispose();
            jeru.Dispose();
            plant.Dispose();
            yarn.Dispose();
            vertexArray.Dispose();
            posTexBuffer.Dispose();
            colBuffer.Dispose();
            indexBuffer.Dispose();

            TrippyLib.Quit();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            Console.WriteLine("keydown: " + e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            Console.WriteLine("kypress:" + e.KeyChar);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Console.WriteLine("mouse down");
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);

            GL.UseProgram(program);
            Matrix4 mat = Matrix4.CreateOrthographicOffCenter(0, this.Width, this.Height, 0, 0, 1);
            GL.UniformMatrix4(projUniform, false, ref mat);
            //mat = Matrix4.CreateScale(Math.Min(this.Width, this.Height) * 0.5f);
            //mat *= Matrix4.CreateTranslation(this.Width / 2f, this.Height / 2f, 0);
            mat = Matrix4.CreateScale(Math.Min(this.Width/(float)fondo.Width, this.Height/(float)fondo.Height));
            GL.UniformMatrix4(viewUniform, false, ref mat);
        }

        private void drawTexture(Texture2D texture, Vector2 center, Vector2 scale, float rotation)
        {
            vertexArray.EnsureBound();

            Matrix4 mat = Matrix4.CreateScale(scale.X * texture.Width, scale.Y * texture.Height, 1f) * Matrix4.CreateRotationZ(rotation) * Matrix4.CreateTranslation(center.X, center.Y, 0);
            GL.UseProgram(program);
            GL.UniformMatrix4(worldUniform, false, ref mat);
            texture.EnsureBoundAndActive();
            GL.Uniform1(texUniform, 0);

            indexBuffer.EnsureBound();

            //GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.DrawElements(PrimitiveType.TriangleStrip, 4, indexBuffer.ElementType, 0);
        }

        private Color4b randomColor()
        {
            byte[] jeje = new byte[4];
            r.NextBytes(jeje);
            return new Color4b(jeje[0], jeje[1], jeje[2], jeje[3]);
        }

        private float wave(float amp, float time, float timeMult, float dephase)
        {
            return amp * (float)Math.Sin(time * timeMult + dephase);
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct Vector5
    {
        public float X, Y, Z, W, V;

        public Vector5(float x, float y, float z, float w, float v)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
            this.V = v;
        }
    }
}
