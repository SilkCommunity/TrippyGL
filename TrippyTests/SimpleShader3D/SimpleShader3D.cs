using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using TrippyGL;
using TrippyGL.ImageSharp;
using TrippyTestBase;

namespace SimpleShader3D
{
    // Renders a simple 3D scene using SimpleShaderProgram-s.
    // The camera can be moved with the mouse, WASD, Q and E.
    // The scene consists of three textureless objects; a dragon, a donut and a sphere.
    // These three objects are illuminated by two lights; a directional light (a pink-ish sun)
    // and a positional light (a strong, white lamp). The lamp can be moved with right click.
    // The lamp has it's own 3D model, which is a textured lamp.
    // The sky is rendered with a special shader. It's bluish-purple with some noise on top.
    // Gamepad controls are also available.

    class SimpleShader3D : TestBase
    {
        Stopwatch stopwatch;

        InputManager3D inputManager;

        SimpleShaderProgram linesProgram;

        VertexBuffer<VertexColor> linesBuffer;
        VertexBuffer<VertexColor> crossBuffer;

        ShaderProgram skyProgram;
        VertexBuffer<VertexPosition> cubeBuffer;

        SimpleShaderProgram modelsProgram;
        VertexBuffer<VertexNormal> dragonBuffer;
        VertexBuffer<VertexNormal> donutBuffer;
        VertexBuffer<VertexNormal> sphereBuffer;

        SimpleShaderProgram lampProgram;
        VertexBuffer<VertexNormalTexture> lampBuffer;
        Texture2D lampTexture;

        Vector3 lampPosition;

        protected override void OnLoad()
        {
            inputManager = new InputManager3D(InputContext)
            {
                CameraMoveSpeed = 2.5f
            };

            Span<VertexColor> crossLines = stackalloc VertexColor[]
            {
                new VertexColor(new Vector3(0, 0, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 1, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(1, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Blue),
                new VertexColor(new Vector3(0, 0, 1), Color4b.Blue),
            };

            crossBuffer = new VertexBuffer<VertexColor>(graphicsDevice, crossLines, BufferUsageARB.StaticDraw);

            Span<VertexPosition> cube = stackalloc VertexPosition[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
            };

            cubeBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, cube, BufferUsageARB.StaticDraw);

            ShaderProgramBuilder skyProgramBuilder = new ShaderProgramBuilder();
            skyProgramBuilder.VertexShaderCode = File.ReadAllText("skyVs.glsl");
            skyProgramBuilder.FragmentShaderCode = File.ReadAllText("skyFs.glsl");
            skyProgramBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            skyProgram = skyProgramBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + skyProgramBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + skyProgramBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + skyProgramBuilder.ProgramLog);

            VertexColor[] linesArray = CreateLines();
            linesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, linesArray, BufferUsageARB.StaticDraw);

            SimpleShaderProgramBuilder programBuilder = new SimpleShaderProgramBuilder()
            {
                VertexColorsEnabled = true
            };
            programBuilder.ConfigureVertexAttribs<VertexColor>();
            linesProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            VertexNormal[] dragonModel = OBJLoader.FromFile<VertexNormal>("dragon.obj");
            dragonBuffer = new VertexBuffer<VertexNormal>(graphicsDevice, dragonModel, BufferUsageARB.StaticDraw);

            VertexNormal[] donutModel = OBJLoader.FromFile<VertexNormal>("donut.obj");
            donutBuffer = new VertexBuffer<VertexNormal>(graphicsDevice, donutModel, BufferUsageARB.StaticDraw);

            VertexNormal[] sphereModel = OBJLoader.FromFile<VertexNormal>("sphere.obj");
            sphereBuffer = new VertexBuffer<VertexNormal>(graphicsDevice, sphereModel, BufferUsageARB.StaticDraw);

            programBuilder = new SimpleShaderProgramBuilder()
            {
                DirectionalLights = 1,
                PositionalLights = 1
            };
            programBuilder.ConfigureVertexAttribs<VertexNormal>();
            modelsProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            lampTexture = Texture2DExtensions.FromFile(graphicsDevice, "lamp_texture.png");

            VertexNormalTexture[] lampModel = OBJLoader.FromFile<VertexNormalTexture>("lamp.obj");
            lampBuffer = new VertexBuffer<VertexNormalTexture>(graphicsDevice, lampModel, BufferUsageARB.StaticDraw);

            programBuilder = new SimpleShaderProgramBuilder()
            {
                TextureEnabled = true
            };
            programBuilder.ConfigureVertexAttribs<VertexNormalTexture>();
            lampProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);
            lampProgram.Texture = lampTexture;
            lampProgram.Color = new Vector4(3, 3, 3, 1);

            Vector3 sunlightDir = new Vector3(-1, -1.5f, -1);
            Vector3 sunColor = new Vector3(0.6f, 0.1f, 1.0f);

            modelsProgram.DirectionalLights[0].Direction = sunlightDir;
            modelsProgram.DirectionalLights[0].DiffuseColor = sunColor;
            modelsProgram.DirectionalLights[0].SpecularColor = sunColor;
            modelsProgram.PositionalLights[0].AttenuationConfig = new Vector3(0, 0.3f, 0.45f);
            modelsProgram.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.3f);
            modelsProgram.SpecularPower = 8;

            skyProgram.Uniforms["sunDirection"].SetValueVec3(Vector3.Normalize(-sunlightDir));
            skyProgram.Uniforms["sunColor"].SetValueVec3(sunColor);

            lampPosition = new Vector3(2.5f, 1, -1.7f);
            modelsProgram.PositionalLights[0].Position = lampPosition;
            lampProgram.World = Matrix4x4.CreateScale(0.015f) * Matrix4x4.CreateTranslation(lampPosition.X, lampPosition.Y - 0.85f, lampPosition.Z);

            graphicsDevice.DepthState = DepthState.Default;
            graphicsDevice.BlendingEnabled = false;

            stopwatch = Stopwatch.StartNew();
        }

        protected override void OnRender(double dt)
        {
            float time = (float)stopwatch.Elapsed.TotalSeconds;
            inputManager.Update((float)dt);

            if ((inputManager.CurrentMouse != null && inputManager.CurrentMouse.IsButtonPressed(MouseButton.Right))
                || (inputManager.CurrentGamepad != null && inputManager.CurrentGamepad.RightBumper().Pressed))
            {
                lampPosition = inputManager.CameraPosition + 2.5f * inputManager.CalculateForwardVector();
                modelsProgram.PositionalLights[0].Position = lampPosition;
                lampProgram.World = Matrix4x4.CreateScale(0.015f) * Matrix4x4.CreateTranslation(lampPosition.X, lampPosition.Y - 0.85f, lampPosition.Z);
            }

            graphicsDevice.FaceCullingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ShaderProgram = skyProgram;
            graphicsDevice.VertexArray = cubeBuffer;
            skyProgram.Uniforms["View"].SetValueMat4(inputManager.CalculateViewMatrixNoTranslation());
            skyProgram.Uniforms["time"].SetValueFloat(time);
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, cubeBuffer.StorageLength);

            graphicsDevice.DepthTestingEnabled = true;
            graphicsDevice.Clear(ClearBufferMask.DepthBufferBit);

            Matrix4x4 view = inputManager.CalculateViewMatrix();
            linesProgram.View = view;
            modelsProgram.View = view;
            lampProgram.View = view;

            graphicsDevice.VertexArray = linesBuffer;
            graphicsDevice.ShaderProgram = linesProgram;
            linesProgram.World = Matrix4x4.Identity;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, linesBuffer.StorageLength);



            graphicsDevice.FaceCullingEnabled = true;
            graphicsDevice.ShaderProgram = modelsProgram;

            graphicsDevice.VertexArray = dragonBuffer;
            modelsProgram.World = Matrix4x4.CreateScale(0.33f) * Matrix4x4.CreateRotationY(time * 0.1f * MathF.PI) * Matrix4x4.CreateTranslation(-1, 0.5f, -2);
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, dragonBuffer.StorageLength);

            graphicsDevice.VertexArray = donutBuffer;
            modelsProgram.World = Matrix4x4.CreateScale(0.7f) * Matrix4x4.CreateTranslation(2, 0, 2);
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, donutBuffer.StorageLength);

            graphicsDevice.VertexArray = sphereBuffer;
            modelsProgram.World = Matrix4x4.CreateScale(1.2f) * Matrix4x4.CreateTranslation(4, 2, -5);
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, sphereBuffer.StorageLength);

            graphicsDevice.VertexArray = lampBuffer;
            graphicsDevice.ShaderProgram = lampProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, lampBuffer.StorageLength);

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ShaderProgram = linesProgram;
            graphicsDevice.VertexArray = crossBuffer;
            Vector3 translation = inputManager.CameraPosition + inputManager.CalculateForwardVector();
            linesProgram.World = Matrix4x4.CreateScale(0.1f) * Matrix4x4.CreateTranslation(translation);
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, crossBuffer.StorageLength);

            Window.SwapBuffers();
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, size.Width / (float)size.Height, 0.1f, 50f);
            linesProgram.Projection = proj;
            skyProgram.Uniforms["Projection"].SetValueMat4(proj);
            modelsProgram.Projection = proj;
            lampProgram.Projection = proj;
        }

        protected override void OnUnload()
        {
            crossBuffer.Dispose();
            linesBuffer.Dispose();
            linesProgram.Dispose();

            modelsProgram.Dispose();
            dragonBuffer.Dispose();
            donutBuffer.Dispose();
            sphereBuffer.Dispose();

            lampProgram.Dispose();
            lampBuffer.Dispose();
            lampTexture.Dispose();

            skyProgram.Dispose();
            cubeBuffer.Dispose();
            graphicsDevice.Dispose();
        }

        private static VertexColor[] CreateLines()
        {
            const float d = 100;

            List<VertexColor> lines = new List<VertexColor>
            {
                new VertexColor(new Vector3(-d, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(d, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(0, -d, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, d, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 0, -d), Color4b.Blue),
                new VertexColor(new Vector3(0, 0, d), Color4b.Blue)
            };

            Color4b darkRed = Color4b.Multiply(Color4b.Red, 0.3f);
            Color4b darkGreen = Color4b.Multiply(Color4b.Lime, 0.3f);
            Color4b darkBlue = Color4b.Multiply(Color4b.Blue, 0.3f);
            for (int i = 1; i < 5; i++)
            {
                lines.Add(new VertexColor(new Vector3(-d, 0, i), darkRed));
                lines.Add(new VertexColor(new Vector3(d, 0, i), darkRed));
                lines.Add(new VertexColor(new Vector3(-d, 0, -i), darkRed));
                lines.Add(new VertexColor(new Vector3(d, 0, -i), darkRed));

                lines.Add(new VertexColor(new Vector3(i, -d, 0), darkGreen));
                lines.Add(new VertexColor(new Vector3(i, d, 0), darkGreen));
                lines.Add(new VertexColor(new Vector3(-i, -d, 0), darkGreen));
                lines.Add(new VertexColor(new Vector3(-i, d, 0), darkGreen));

                lines.Add(new VertexColor(new Vector3(i, 0, -d), darkBlue));
                lines.Add(new VertexColor(new Vector3(i, 0, d), darkBlue));
                lines.Add(new VertexColor(new Vector3(-i, 0, -d), darkBlue));
                lines.Add(new VertexColor(new Vector3(-i, 0, d), darkBlue));
            }

            return lines.ToArray();
        }
    }
}
