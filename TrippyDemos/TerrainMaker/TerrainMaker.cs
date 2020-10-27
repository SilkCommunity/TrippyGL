using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using TrippyGL;
using TrippyGL.ImageSharp;
using TrippyTestBase;

namespace TerrainMaker
{
    class TerrainMaker : TestBase
    {
        private const DepthStencilFormat PreferredDepthFormat = DepthStencilFormat.Depth24;
        public static Stopwatch stopwatch = new Stopwatch();
        public static Random random = new Random();

        InputManager3D inputManager;

        ShaderProgram waterProgram;
        ShaderUniform waterViewUniform, waterSunlightColor, waterSunlightDir, waterCameraPos, waterDistortOffset;

        ShaderProgram terrainProgram;
        ShaderUniform terrainViewUniform, terrainCameraPosUniform;

        Framebuffer2D waterReflectFbo, waterRefractFbo;
        Texture2D waterDistortMap, waterNormalsMap;

        SimpleShaderProgram linesProgram;
        VertexBuffer<VertexColor> linesBuffer;

        Framebuffer2D mainFramebuffer;
        Texture2D mainFramebufferDepthTexture;

        SimpleShaderProgram textureProgram;
        TextureBatcher textureBatcher;

        ShaderProgram underwaterShader;
        ShaderUniform underwaterColorUniform, underwaterViewDistanceUniform;
        ShaderUniform underwaterShaderTextureUniform, underwaterShaderDepthUniform;

        ChunkManager chunkManager;

        public TerrainMaker() : base(null, 0, true) { }

        protected override void OnLoad()
        {
            stopwatch.Restart();
            inputManager = new InputManager3D(InputContext);

            Vector3 startCoords = new Vector3(TerrainGenerator.ChunkSize / 2f, 0f, TerrainGenerator.ChunkSize / 2f);
            startCoords.Y = Math.Max(NoiseGenerator.GenHeight(new Vector2(startCoords.X, startCoords.Z)), 0) + 32f;
            inputManager.CameraPosition = startCoords;

            terrainProgram = ShaderProgram.FromFiles<VertexNormalColor>(graphicsDevice, "data/terrainVs.glsl", "data/terrainFs.glsl", "vPosition", "vNormal", "vColor");
            terrainViewUniform = terrainProgram.Uniforms["View"];
            terrainCameraPosUniform = terrainProgram.Uniforms["cameraPos"];

            waterDistortMap = Texture2DExtensions.FromFile(graphicsDevice, "data/distortMap.png");
            waterNormalsMap = Texture2DExtensions.FromFile(graphicsDevice, "data/normalMap.png");
            waterDistortMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            waterDistortMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            waterNormalsMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            waterNormalsMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            waterReflectFbo = new Framebuffer2D(graphicsDevice, (uint)Window.Size.Width, (uint)Window.Size.Height, PreferredDepthFormat);
            waterRefractFbo = new Framebuffer2D(graphicsDevice, (uint)Window.Size.Width, (uint)Window.Size.Height, PreferredDepthFormat, 0, TextureImageFormat.Color4b, true);
            waterReflectFbo.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            waterRefractFbo.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            waterProgram = ShaderProgram.FromFiles<VertexNormalColor>(graphicsDevice, "data/waterVs.glsl", "data/waterFs.glsl", "vPosition");
            waterProgram.Uniforms["distortMap"].SetValueTexture(waterDistortMap);
            waterProgram.Uniforms["normalsMap"].SetValueTexture(waterNormalsMap);
            waterProgram.Uniforms["reflectSamp"].SetValueTexture(waterReflectFbo);
            waterProgram.Uniforms["refractSamp"].SetValueTexture(waterRefractFbo);
            waterRefractFbo.Framebuffer.TryGetTextureAttachment(FramebufferAttachmentPoint.Depth, out FramebufferTextureAttachment rtdpt);
            waterProgram.Uniforms["depthSamp"].SetValueTexture(rtdpt.Texture);
            waterCameraPos = waterProgram.Uniforms["cameraPos"];
            waterDistortOffset = waterProgram.Uniforms["distortOffset"];
            waterSunlightColor = waterProgram.Uniforms["sunlightColor"];
            waterSunlightColor.SetValueVec3(1, 0.9f, 0.9f);
            waterSunlightDir = waterProgram.Uniforms["sunlightDir"];
            waterSunlightDir.SetValueVec3(Vector3.UnitX);
            waterViewUniform = waterProgram.Uniforms["View"];

            Span<VertexColor> lines = stackalloc VertexColor[]
            {
                new VertexColor(new Vector3(0, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(1, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 1, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Blue),
                new VertexColor(new Vector3(0, 0, 1), Color4b.Blue),
            };

            linesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, lines, BufferUsageARB.StaticDraw);
            linesProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            mainFramebuffer = new Framebuffer2D(graphicsDevice, (uint)Window.Size.Width, (uint)Window.Size.Height, PreferredDepthFormat, 0, TextureImageFormat.Color4b, true);
            mainFramebuffer.TryGetDepthTexture(out mainFramebufferDepthTexture);

            textureProgram = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);

            underwaterShader = ShaderProgram.FromFiles<VertexColorTexture>(graphicsDevice, "data/postprocessVs.glsl", "data/underwaterFs.glsl", "vPosition", "vColor", "vTexCoords");
            underwaterShaderTextureUniform = underwaterShader.Uniforms["textureSamp"];
            underwaterShaderDepthUniform = underwaterShader.Uniforms["depthSamp"];
            underwaterViewDistanceUniform = underwaterShader.Uniforms["maxDistance"];
            underwaterColorUniform = underwaterShader.Uniforms["waterColor"];

            textureBatcher = new TextureBatcher(graphicsDevice);

            chunkManager = new ChunkManager(12, (int)inputManager.CameraPosition.X / TerrainGenerator.ChunkSize, (int)inputManager.CameraPosition.Z / TerrainGenerator.ChunkSize);

            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.DepthState = new DepthState(true, DepthFunction.Lequal);
            graphicsDevice.ClearColor = Vector4.UnitW;
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);

            inputManager.CameraMoveSpeed = inputManager.CurrentKeyboard.IsKeyPressed(Key.ControlLeft) ? 175 : 30;
            inputManager.Update((float)dt);
            float th = NoiseGenerator.GenHeight(new Vector2(inputManager.CameraPosition.X, inputManager.CameraPosition.Z));
            inputManager.CameraPosition.Y = Math.Max(inputManager.CameraPosition.Y, th + 4);
            //Window.Title = inputManager.CameraPosition.ToString();
            if (!inputManager.CurrentKeyboard.IsKeyPressed(Key.G))
                chunkManager.SetCenterChunk((int)MathF.Floor(inputManager.CameraPosition.X / TerrainGenerator.ChunkSize), (int)MathF.Floor(inputManager.CameraPosition.Z / TerrainGenerator.ChunkSize));
            chunkManager.ProcessChunks(graphicsDevice);
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int n)
        {
            if (key == Key.P)
            {
                chunkManager.ChunkRenderRadius++;
                Console.WriteLine("ChunkRenderRadius set to " + chunkManager.ChunkRenderRadius);
                SetNearFarPlanes();
            }
            else if (key == Key.O && chunkManager.ChunkRenderRadius > 1)
            {
                chunkManager.ChunkRenderRadius--;
                Console.WriteLine("ChunkRenderRadius set to " + chunkManager.ChunkRenderRadius);
                SetNearFarPlanes();
            }
        }

        private void RenderWaterFbos(in Matrix4x4 view)
        {
            Matrix4x4 invertedView = Matrix4x4.CreateTranslation(-inputManager.CameraPosition.X, inputManager.CameraPosition.Y, -inputManager.CameraPosition.Z);
            invertedView *= Matrix4x4.CreateRotationY(inputManager.CameraRotationY + MathF.PI / 2f);
            invertedView *= Matrix4x4.CreateRotationX(inputManager.CameraRotationX);

            graphicsDevice.FaceCullingEnabled = true;
            graphicsDevice.DepthTestingEnabled = true;
            graphicsDevice.ShaderProgram = terrainProgram;

            graphicsDevice.DrawFramebuffer = waterRefractFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            terrainViewUniform.SetValueMat4(view);
            terrainCameraPosUniform.SetValueVec3(inputManager.CameraPosition);
            if (inputManager.CameraPosition.Y >= 0)
                chunkManager.RenderAllUnderwaters(graphicsDevice);
            else
                chunkManager.RenderAllTerrains(graphicsDevice);

            graphicsDevice.DrawFramebuffer = waterReflectFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            terrainViewUniform.SetValueMat4(invertedView);
            terrainCameraPosUniform.SetValueVec3(inputManager.CameraPosition * new Vector3(1, -1, 1));
            if (inputManager.CameraPosition.Y >= 0)
                chunkManager.RenderAllTerrains(graphicsDevice);
            else
                chunkManager.RenderAllUnderwaters(graphicsDevice);
        }

        protected override void OnRender(double dt)
        {
            float time = (float)stopwatch.Elapsed.TotalSeconds;

            Matrix4x4 view = inputManager.CalculateViewMatrix();
            Vector3 cameraForward = inputManager.CalculateForwardVector();
            if (!inputManager.CurrentKeyboard.IsKeyPressed(Key.H))
            {
                chunkManager.CameraDirection = cameraForward;
                chunkManager.CameraPosition = inputManager.CameraPosition;
            }

            RenderWaterFbos(view);

            graphicsDevice.DrawFramebuffer = mainFramebuffer;
            graphicsDevice.ClearColor = Color4b.SkyBlue;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            graphicsDevice.FaceCullingEnabled = true;
            graphicsDevice.ShaderProgram = terrainProgram;
            terrainViewUniform.SetValueMat4(view);
            terrainCameraPosUniform.SetValueVec3(inputManager.CameraPosition);
            if (inputManager.CameraPosition.Y >= 0)
                chunkManager.RenderAllTerrains(graphicsDevice);
            else
                chunkManager.RenderAllUnderwaters(graphicsDevice);

            graphicsDevice.FaceCullingEnabled = false;

            graphicsDevice.ShaderProgram = waterProgram;
            waterCameraPos.SetValueVec3(inputManager.CameraPosition);
            waterDistortOffset.SetValueVec2(time * 0.2178f % 1, time * 0.2853f % 1);
            waterViewUniform.SetValueMat4(view);
            chunkManager.RenderAllUnderwaters(graphicsDevice);

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ShaderProgram = linesProgram;
            linesProgram.View = view;
            linesProgram.World = Matrix4x4.CreateScale(0.05f) * Matrix4x4.CreateTranslation(inputManager.CameraPosition + cameraForward);
            graphicsDevice.VertexArray = linesBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, linesBuffer.StorageLength);

            graphicsDevice.Framebuffer = null;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            if (inputManager.CameraPosition.Y >= 0)
                textureBatcher.SetShaderProgram(textureProgram);
            else
            {
                textureBatcher.SetShaderProgram(underwaterShader, underwaterShaderTextureUniform);
                underwaterShaderDepthUniform.SetValueTexture(mainFramebufferDepthTexture);
                underwaterViewDistanceUniform.SetValueFloat(Math.Max(128 + 3 * inputManager.CameraPosition.Y, 20));
                underwaterColorUniform.SetValueVec3(Color4b.DeepSkyBlue.ToVector3() * (1 - Math.Clamp((-5 - inputManager.CameraPosition.Y) / 40f, 0, 0.7f)));
            }
            textureBatcher.Begin(BatcherBeginMode.OnTheFly);
            textureBatcher.Draw(mainFramebuffer, new Vector2(0, mainFramebuffer.Height), null, Color4b.White, new Vector2(1, -1), 0f);
            textureBatcher.End();

            Window.SwapBuffers();
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.Viewport = new Viewport(0, 0, (uint)size.Width, (uint)size.Height);
            SetNearFarPlanes();

            Matrix4x4 proj = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
            textureProgram.Projection = proj;
            underwaterShader.Uniforms["Projection"].SetValueMat4(proj);

            waterRefractFbo.Resize((uint)size.Width, (uint)size.Height);
            waterReflectFbo.Resize((uint)size.Width, (uint)size.Height);
            mainFramebuffer.Resize((uint)size.Width, (uint)size.Height);
        }

        private void SetNearFarPlanes()
        {
            float nearPlane = 0.01f;
            float farPlane = chunkManager.ChunkRenderRadius * TerrainGenerator.ChunkSize;
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, (float)Window.Size.Width / Window.Size.Height, nearPlane, farPlane);
            terrainProgram.Uniforms["Projection"].SetValueMat4(proj);
            waterProgram.Uniforms["Projection"].SetValueMat4(proj);
            linesProgram.Projection = proj;

            waterProgram.Uniforms["nearPlane"].SetValueFloat(nearPlane);
            waterProgram.Uniforms["farPlane"].SetValueFloat(farPlane);
            underwaterShader.Uniforms["nearPlane"].SetValueFloat(nearPlane);
            underwaterShader.Uniforms["farPlane"].SetValueFloat(farPlane);

            float farFog = chunkManager.ChunkRenderRadius * TerrainGenerator.ChunkSize;
            float nearFog = farFog - 2 * TerrainGenerator.ChunkSize;
            terrainProgram.Uniforms["nearFog"].SetValueFloat(nearFog);
            terrainProgram.Uniforms["farFog"].SetValueFloat(farFog);
            waterProgram.Uniforms["nearFog"].SetValueFloat(nearFog);
            waterProgram.Uniforms["farFog"].SetValueFloat(farFog);
        }

        protected override void OnUnload()
        {
            terrainProgram.Dispose();
        }
    }
}
