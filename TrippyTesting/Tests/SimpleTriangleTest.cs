﻿using System;
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
    class SimpleTriangleTest : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        float time;

        ShaderProgram program;
        VertexBuffer<VertexColor> buffer;

        public SimpleTriangleTest() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "Simple Triangle", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            TrippyLib.Init();

            Console.WriteLine(String.Concat("GL Version: ", TrippyLib.GLMajorVersion, ".", TrippyLib.GLMinorVersion));
            Console.WriteLine("GL Version String: " + TrippyLib.GLVersion);
            Console.WriteLine("GL Vendor: " + TrippyLib.GLVendor);
            Console.WriteLine("GL Renderer: " + TrippyLib.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + TrippyLib.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + TrippyLib.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + TrippyLib.MaxTextureSize);
            Console.WriteLine("GL MaxSamples:" + TrippyLib.MaxSamples);
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            VertexColor[] data = new VertexColor[]
            {
                new VertexColor(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(0, 0.5f, 0), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 0, 255, 255)),
            };

            buffer = new VertexBuffer<VertexColor>(data.Length, data, BufferUsageHint.StaticDraw);

            program = new ShaderProgram();
            program.AddVertexShader("#version 400\r\nin vec3 vPos; in vec4 vCol; out vec4 fCol; void main() { gl_Position=vec4(vPos, 1.0); fCol = vCol; }");
            program.AddFragmentShader("#version 400\r\nin vec4 fCol; out vec4 FragColor; void main() { FragColor = fCol; }");
            program.SpecifyVertexAttribs<VertexColor>(new string[] { "vPos", "vCol" });
            program.LinkProgram();
            Console.WriteLine("Program info log: \n" + GL.GetProgramInfoLog(program.Handle) + "\n[END OF LOG]");
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

            States.EnsureShaderProgramInUse(program);
            buffer.EnsureArrayBound();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 3);

            SwapBuffers();

            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            buffer.Dispose();
            program.Dispose();

            TrippyLib.Quit();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }
    }
}