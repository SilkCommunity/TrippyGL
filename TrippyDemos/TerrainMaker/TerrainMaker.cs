using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using TrippyGL;
using TrippyGL.ImageSharp;
using TrippyTestBase;

namespace TerrainMaker
{
    class TerrainMaker : TestBase
    {
        private const DepthStencilFormat PreferredDepthFormat = DepthStencilFormat.Depth32f;
        public static Stopwatch stopwatch = new Stopwatch();
        public static Random random = new Random();

        const float MinCameraSpeed = 10, MaxCameraSpeed = 2500;

        InputManager3D inputManager;

        VertexBuffer<VertexPosition> skyBuffer;
        ShaderProgram skyProgram;
        ShaderUniform skyViewUniform, skySunDirectionUniform, skySunColorUniform;

        ShaderProgram waterProgram;
        ShaderUniform waterViewUniform, waterSunlightColorUniform, waterSunlightDirUniform, waterCameraPosUniform, waterDistortOffsetUniform;

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

        protected override void OnLoad()
        {
            stopwatch.Restart();
            inputManager = new InputManager3D(InputContext);

            chunkManager = new ChunkManager(GeneratorSeed.Default, 12, 0, 0);

            Vector3 startCoords = new Vector3(TerrainGenerator.ChunkSize / 2f, 0f, TerrainGenerator.ChunkSize / 2f);
            startCoords.Y = Math.Max(NoiseGenerator.GenHeight(chunkManager.GeneratorSeed, new Vector2(startCoords.X, startCoords.Z)), 0) + 32f;
            inputManager.CameraPosition = startCoords;
            inputManager.CameraMoveSpeed = 100;

            Span<VertexPosition> skyVertices = stackalloc VertexPosition[]
            {
                new Vector3(0, -1, 1),
                new Vector3(1, -1, -1),
                new Vector3(-1, -1, -1),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 1),
                new Vector3(1, -1, -1),
            };

            skyBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, skyVertices, BufferUsage.StaticDraw);
            skyProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "data/skyVs.glsl", "data/skyFs.glsl", "vPosition");
            skySunDirectionUniform = skyProgram.Uniforms["sunDirection"];
            skySunColorUniform = skyProgram.Uniforms["sunColor"];
            skyViewUniform = skyProgram.Uniforms["View"];

            terrainProgram = ShaderProgram.FromFiles<TerrainVertex>(graphicsDevice, "data/terrainVs.glsl", "data/terrainFs.glsl", "vPosition", "vNormal", "vColor", "vLightingConfig");
            terrainViewUniform = terrainProgram.Uniforms["View"];
            terrainCameraPosUniform = terrainProgram.Uniforms["cameraPos"];

            waterDistortMap = Texture2DExtensions.FromFile(graphicsDevice, "data/distortMap.png");
            waterNormalsMap = Texture2DExtensions.FromFile(graphicsDevice, "data/normalMap.png");
            waterDistortMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            waterDistortMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            waterNormalsMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            waterNormalsMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            waterReflectFbo = new Framebuffer2D(graphicsDevice, (uint)Window.Size.X, (uint)Window.Size.Y, PreferredDepthFormat);
            waterRefractFbo = new Framebuffer2D(graphicsDevice, (uint)Window.Size.X, (uint)Window.Size.Y, PreferredDepthFormat, 0, TextureImageFormat.Color4b, true);
            waterReflectFbo.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            waterRefractFbo.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            waterProgram = ShaderProgram.FromFiles<VertexNormalColor>(graphicsDevice, "data/waterVs.glsl", "data/waterFs.glsl", "vPosition");
            waterProgram.Uniforms["distortMap"].SetValueTexture(waterDistortMap);
            waterProgram.Uniforms["normalsMap"].SetValueTexture(waterNormalsMap);
            waterProgram.Uniforms["reflectSamp"].SetValueTexture(waterReflectFbo);
            waterProgram.Uniforms["refractSamp"].SetValueTexture(waterRefractFbo);
            waterRefractFbo.Framebuffer.TryGetTextureAttachment(FramebufferAttachmentPoint.Depth, out FramebufferTextureAttachment rtdpt);
            ((Texture2D)rtdpt.Texture).SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            waterProgram.Uniforms["depthSamp"].SetValueTexture(rtdpt.Texture);
            waterCameraPosUniform = waterProgram.Uniforms["cameraPos"];
            waterDistortOffsetUniform = waterProgram.Uniforms["distortOffset"];
            waterSunlightColorUniform = waterProgram.Uniforms["sunlightColor"];
            waterSunlightDirUniform = waterProgram.Uniforms["sunlightDir"];
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

            linesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, lines, BufferUsage.StaticDraw);
            linesProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            mainFramebuffer = new Framebuffer2D(graphicsDevice, (uint)Window.Size.X, (uint)Window.Size.Y, PreferredDepthFormat, 0, TextureImageFormat.Color4b, true);
            mainFramebuffer.TryGetDepthTexture(out mainFramebufferDepthTexture);
            mainFramebuffer.Texture.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            mainFramebuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            mainFramebufferDepthTexture.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            mainFramebufferDepthTexture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            textureProgram = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);

            underwaterShader = ShaderProgram.FromFiles<VertexColorTexture>(graphicsDevice, "data/postprocessVs.glsl", "data/underwaterFs.glsl", "vPosition", "vColor", "vTexCoords");
            underwaterShaderTextureUniform = underwaterShader.Uniforms["textureSamp"];
            underwaterShaderDepthUniform = underwaterShader.Uniforms["depthSamp"];
            underwaterViewDistanceUniform = underwaterShader.Uniforms["maxDistance"];
            underwaterColorUniform = underwaterShader.Uniforms["waterColor"];

            textureBatcher = new TextureBatcher(graphicsDevice);

            SetSun(new Vector3(-1, 0.6f, 0.5f), Color4b.LightGoldenrodYellow.ToVector3());

            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.DepthState = new DepthState(true, DepthFunction.LessOrEqual);
            graphicsDevice.ClearColor = Vector4.UnitW;
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);
            float dtfloat = (float)dt;

            if (inputManager.CurrentGamepad != null)
            {
                const float changespd = 200;
                if (inputManager.CurrentGamepad.DPadUp().Pressed)
                    inputManager.CameraMoveSpeed = Math.Clamp(inputManager.CameraMoveSpeed + changespd * dtfloat, MinCameraSpeed, MaxCameraSpeed);
                if (inputManager.CurrentGamepad.DPadDown().Pressed)
                    inputManager.CameraMoveSpeed = Math.Clamp(inputManager.CameraMoveSpeed - changespd * dtfloat, MinCameraSpeed, MaxCameraSpeed);
            }

            inputManager.Update(dtfloat);

            float th = NoiseGenerator.GenHeight(chunkManager.GeneratorSeed, new Vector2(inputManager.CameraPosition.X, inputManager.CameraPosition.Z));
            inputManager.CameraPosition.Y = Math.Max(inputManager.CameraPosition.Y, th + 4);
            Window.Title = inputManager.CameraPosition.ToString();
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
            else if (key == Key.R)
            {
                chunkManager.GeneratorSeed = new GeneratorSeed(random);
            }
        }

        protected override void OnMouseScroll(IMouse sender, ScrollWheel scroll)
        {
            float delta = scroll.Y * 12.5f;
            inputManager.CameraMoveSpeed = Math.Clamp(inputManager.CameraMoveSpeed + delta, MinCameraSpeed, MaxCameraSpeed);
        }

        private void SetSun(Vector3 sunDirection, Vector3 sunColor)
        {
            sunDirection = Vector3.Normalize(sunDirection);

            terrainProgram.Uniforms["sunDirection"].SetValueVec3(sunDirection);
            skySunDirectionUniform.SetValueVec3(sunDirection);
            skySunColorUniform.SetValueVec3(sunColor);
            waterSunlightDirUniform.SetValueVec3(-sunDirection);
            waterSunlightColorUniform.SetValueVec3(sunColor);
        }

        private void DrawSky(in Matrix4x4 viewNoTranslation)
        {
            graphicsDevice.FaceCullingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.ShaderProgram = skyProgram;
            skyViewUniform.SetValueMat4(viewNoTranslation);
            graphicsDevice.VertexArray = skyBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, skyBuffer.StorageLength);
            graphicsDevice.Clear(ClearBuffers.Depth);
        }

        private void RenderWaterFbos(in Matrix4x4 view, in Matrix4x4 viewNoTranslation)
        {
            Matrix4x4 invertedView = Matrix4x4.CreateTranslation(-inputManager.CameraPosition.X, inputManager.CameraPosition.Y, -inputManager.CameraPosition.Z);
            invertedView *= Matrix4x4.CreateRotationY(inputManager.CameraRotationY + MathF.PI / 2f);
            invertedView *= Matrix4x4.CreateRotationX(inputManager.CameraRotationX);

            Matrix4x4 invertedViewNoTranslation = invertedView;
            invertedViewNoTranslation.Translation = Vector3.Zero;

            graphicsDevice.DrawFramebuffer = waterRefractFbo;
            DrawSky(viewNoTranslation);
            graphicsDevice.FaceCullingEnabled = true;
            graphicsDevice.DepthTestingEnabled = true;
            graphicsDevice.BlendingEnabled = true;
            graphicsDevice.ShaderProgram = terrainProgram;
            terrainViewUniform.SetValueMat4(view);
            terrainCameraPosUniform.SetValueVec3(inputManager.CameraPosition);
            if (inputManager.CameraPosition.Y >= 0)
                chunkManager.RenderAllUnderwaters(graphicsDevice);
            else
                chunkManager.RenderAllTerrains(graphicsDevice);

            graphicsDevice.DrawFramebuffer = waterReflectFbo;
            DrawSky(invertedViewNoTranslation);
            graphicsDevice.ShaderProgram = terrainProgram;
            graphicsDevice.FaceCullingEnabled = true;
            graphicsDevice.DepthTestingEnabled = true;
            graphicsDevice.BlendingEnabled = true;
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
            Matrix4x4 viewNoTranslation = view;
            viewNoTranslation.Translation = Vector3.Zero;
            Vector3 cameraForward = inputManager.CalculateForwardVector();
            if (!inputManager.CurrentKeyboard.IsKeyPressed(Key.H))
            {
                chunkManager.CameraDirection = cameraForward;
                chunkManager.CameraPosition = inputManager.CameraPosition;
            }

            RenderWaterFbos(view, viewNoTranslation);

            graphicsDevice.DrawFramebuffer = mainFramebuffer;
            DrawSky(viewNoTranslation);

            graphicsDevice.BlendingEnabled = true;
            graphicsDevice.DepthTestingEnabled = true;
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
            waterCameraPosUniform.SetValueVec3(inputManager.CameraPosition);
            waterDistortOffsetUniform.SetValueVec2(time * 0.2178f % 1, time * 0.2853f % 1);
            waterViewUniform.SetValueMat4(view);
            chunkManager.RenderAllUnderwaters(graphicsDevice);

            graphicsDevice.DepthTestingEnabled = false;

            if (!inputManager.CurrentKeyboard.IsKeyPressed(Key.C))
            {
                graphicsDevice.ShaderProgram = linesProgram;
                linesProgram.View = view;
                linesProgram.World = Matrix4x4.CreateScale(0.05f) * Matrix4x4.CreateTranslation(inputManager.CameraPosition + cameraForward);
                graphicsDevice.VertexArray = linesBuffer;
                graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, linesBuffer.StorageLength);
            }

            graphicsDevice.Framebuffer = null;
            graphicsDevice.Clear(ClearBuffers.Color);
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
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.Viewport = new Viewport(0, 0, (uint)size.X, (uint)size.Y);
            SetNearFarPlanes();

            Matrix4x4 proj = Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, 0, 1);
            textureProgram.Projection = proj;
            underwaterShader.Uniforms["Projection"].SetValueMat4(proj);

            waterRefractFbo.Resize((uint)size.X, (uint)size.Y);
            waterReflectFbo.Resize((uint)size.X, (uint)size.Y);
            mainFramebuffer.Resize((uint)size.X, (uint)size.Y);
        }

        private void SetNearFarPlanes()
        {
            float nearPlane = 0.01f;
            float farPlane = chunkManager.ChunkRenderRadius * TerrainGenerator.ChunkSize;
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, (float)Window.Size.X / Window.Size.Y, nearPlane, farPlane);
            terrainProgram.Uniforms["Projection"].SetValueMat4(proj);
            waterProgram.Uniforms["Projection"].SetValueMat4(proj);
            skyProgram.Uniforms["Projection"].SetValueMat4(proj);
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
            chunkManager.Dispose();
            // All remaining GraphicsResource-s get automatically disposed.
            // the GraphicsDevice gets disposed by the base class.
        }
    }
}
