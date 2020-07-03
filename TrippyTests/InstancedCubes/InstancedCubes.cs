﻿using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace InstancedCubes
{
    // Draws a bunch of rotating 3D colored opaque cubes in front of a static
    // camera, all in a single draw call by using instanced rendering.

    class InstancedCubes
    {
        Stopwatch stopwatch;
        IWindow window;

        GraphicsDevice graphicsDevice;

        BufferObject buffer;
        VertexDataBufferSubset<VertexColor> vertexData;
        VertexDataBufferSubset<Vector4> offsetData;
        VertexArray vertexArray;

        ShaderProgram shaderProgram;

        public InstancedCubes()
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

            // The vertices of a cube, with some nice colors
            Span<VertexColor> vertex = stackalloc VertexColor[] {
                new VertexColor(new Vector3(-0.5f, -0.5f, -0.5f), Color4b.LightBlue),//4
                new VertexColor(new Vector3(-0.5f, -0.5f, 0.5f), Color4b.Lime),//3
                new VertexColor(new Vector3(-0.5f, 0.5f, -0.5f), Color4b.White),//7
                new VertexColor(new Vector3(-0.5f, 0.5f, 0.5f), Color4b.Black),//8
                new VertexColor(new Vector3(0.5f, 0.5f, 0.5f), Color4b.Blue),//5
                new VertexColor(new Vector3(-0.5f, -0.5f, 0.5f), Color4b.Lime),//3
                new VertexColor(new Vector3(0.5f, -0.5f, 0.5f), Color4b.Red),//1
                new VertexColor(new Vector3(-0.5f, -0.5f, -0.5f), Color4b.LightBlue),//4
                new VertexColor(new Vector3(0.5f, -0.5f, -0.5f), Color4b.Yellow),//2
                new VertexColor(new Vector3(-0.5f, 0.5f, -0.5f), Color4b.White),//7
                new VertexColor(new Vector3(0.5f, 0.5f, -0.5f), Color4b.Pink),//6
                new VertexColor(new Vector3(0.5f, 0.5f, 0.5f), Color4b.Blue),//5
                new VertexColor(new Vector3(0.5f, -0.5f, -0.5f), Color4b.Yellow),//2
                new VertexColor(new Vector3(0.5f, -0.5f, 0.5f), Color4b.Red),//1
            };

            // The data where we want each instance of the cube to be rendered.
            // The xyz values of the vector is an offset, the w value is the scale.
            Span<Vector4> offsets = stackalloc Vector4[]
            {
                new Vector4(0, 0, 0, 1),
                new Vector4(0, 0.75f, 0, 0.5f),

                new Vector4(2, -0.2f, 0, 0.5f),
                new Vector4(-2, -0.2f, 0, 0.5f),
                new Vector4(0, -0.2f, -2, 0.5f),
                new Vector4(0, -0.2f, 2, 0.5f),
                new Vector4(2, -0.2f, -2, 0.5f),
                new Vector4(2, -0.2f, 2, 0.5f),
                new Vector4(-2, -0.2f, -2, 0.5f),
                new Vector4(-2, -0.2f, 2, 0.5f),

                new Vector4(3, -0.3f, 0, 0.25f),
                new Vector4(-3, -0.3f, 0, 0.25f),
                new Vector4(0, -0.3f, -3, 0.25f),
                new Vector4(0, -0.3f, 3, 0.25f),
                new Vector4(3, -0.3f, -3, 0.25f),
                new Vector4(3, -0.3f, 3, 0.25f),
                new Vector4(-3, -0.3f, -3, 0.25f),
                new Vector4(-3, -0.3f, 3, 0.25f),
            };

            // If you get bored and want a lot of cubes, uncomment this section.
            /*Random r = new Random();
            Vector4[] offsetsList = new Vector4[1000000];
            for (int i = 0; i < offsetsList.Length; i++)
            {
                offsetsList[i] = new Vector4(
                    -8f + 16f * (float)r.NextDouble(),
                    -8f + 16f * (float)r.NextDouble(),
                    -3 + 16f * (float)r.NextDouble(),
                    0.04f + 0.1f * (float)r.NextDouble()
                );
            }
            offsets = offsetsList;*/

            // We create a BufferObject with enough storage for all the vertex attribute data
            buffer = new BufferObject(graphicsDevice, (uint)(vertex.Length * VertexColor.SizeInBytes + offsets.Length * 16), BufferUsageARB.StaticDraw);

            // The attributes of the vertices will be in two different buffer subsets (inside the same BufferObject).
            // The first subset contains the vertex data of the cube. This one cube will be rendered in multiple instances.
            vertexData = new VertexDataBufferSubset<VertexColor>(buffer, 0, (uint)vertex.Length, vertex);

            // The second subset contains the data for each instance. For each one of these
            // Vector4-s, another cube will be rendered.
            offsetData = new VertexDataBufferSubset<Vector4>(buffer, vertexData.NextByteInBuffer, (uint)offsets.Length, offsets);

            // The vertex specification will specify first a FloatVec3, followed by 4 normalized
            // bytes (the color), and as third attribute we will have the offset data (which is per instance)
            // whose pointer will only advance once per instance, rather than for each vertex.
            vertexArray = new VertexArray(graphicsDevice, new VertexAttribSource[] {
                new VertexAttribSource(vertexData, AttributeType.FloatVec3),
                new VertexAttribSource(vertexData, AttributeType.FloatVec4, true, VertexAttribPointerType.UnsignedByte),
                new VertexAttribSource(offsetData, AttributeType.FloatVec4, 1)
            });

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs(new SpecifiedShaderAttrib[] {
                new SpecifiedShaderAttrib("vPosition", AttributeType.FloatVec3),
                new SpecifiedShaderAttrib("vColor", AttributeType.FloatVec4),
                new SpecifiedShaderAttrib("vOffset", AttributeType.FloatVec4)
            });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            shaderProgram.Uniforms["View"].SetValueMat4(Matrix4x4.CreateLookAt(new Vector3(0, 5, -4), Vector3.Zero, Vector3.UnitY));

            graphicsDevice.DepthState = DepthTestingState.Default;
            graphicsDevice.BlendingEnabled = false;

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

            graphicsDevice.ClearDepth = 1f;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.CreateRotationY((float)stopwatch.Elapsed.TotalSeconds * 3.1415f));

            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexArray;
            graphicsDevice.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, vertexData.StorageLength, offsetData.StorageLength);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, window.Size.Width / (float)window.Size.Height, 0.01f, 100f));
        }

        private void OnWindowClosing()
        {
            vertexArray.Dispose();
            buffer.Dispose();
            shaderProgram.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
