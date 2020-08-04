using System;
using Silk.NET.Input.Common;
using System.Numerics;
using System.Drawing;

namespace TrippyTestBase
{
    public sealed class InputManager3D
    {
        public readonly IInputContext InputContext;

        private IKeyboard currentKeyboard;
        private IMouse currentMouse;
        private IGamepad currentGamepad;

        public bool LockMouseWhileRotating = true;

        public float MouseSensitivity = 0.005f;
        public float CameraThumbstickSensitivity = 3;
        public float CameraMoveSpeed = 1;

        private PointF lastMousePos;

        public float CameraRotationY;
        public float CameraRotationX;

        public Vector3 CameraPosition;

        public InputManager3D(IInputContext inputContext)
        {
            InputContext = inputContext ?? throw new ArgumentNullException(nameof(inputContext));

            inputContext.ConnectionChanged += OnInputContextConnectionChanged;

            currentKeyboard = InputContext.Keyboards.Count == 0 ? null : InputContext.Keyboards[0];
            currentMouse = InputContext.Mice.Count == 0 ? null : InputContext.Mice[0];
            currentGamepad = InputContext.Gamepads.Count == 0 ? null : InputContext.Gamepads[0];

            CameraRotationX = 0;
            CameraRotationY = 0;
            CameraPosition = Vector3.Zero;
        }

        public void Update(float dtSeconds)
        {
            if (currentMouse != null)
            {
                if (currentMouse.IsButtonPressed(MouseButton.Left))
                {
                    CameraRotationY += (currentMouse.Position.X - lastMousePos.X) * MouseSensitivity;
                    CameraRotationX = Math.Clamp(CameraRotationX + (currentMouse.Position.Y - lastMousePos.Y) * -MouseSensitivity, -1.57f, 1.57f);
                    if (LockMouseWhileRotating)
                        currentMouse.Position = lastMousePos;
                }

                lastMousePos = currentMouse.Position;
            }

            if (currentKeyboard != null)
            {
                float sinY = MathF.Sin(CameraRotationY);
                float cosY = MathF.Cos(CameraRotationY);

                if (currentKeyboard.IsKeyPressed(Key.W))
                    CameraPosition += new Vector3(cosY, 0, sinY) * CameraMoveSpeed * dtSeconds;
                if (currentKeyboard.IsKeyPressed(Key.S))
                    CameraPosition -= new Vector3(cosY, 0, sinY) * CameraMoveSpeed * dtSeconds;

                if (currentKeyboard.IsKeyPressed(Key.A))
                    CameraPosition += new Vector3(sinY, 0, -cosY) * CameraMoveSpeed * dtSeconds;
                if (currentKeyboard.IsKeyPressed(Key.D))
                    CameraPosition -= new Vector3(sinY, 0, -cosY) * CameraMoveSpeed * dtSeconds;

                if (currentKeyboard.IsKeyPressed(Key.E))
                    CameraPosition.Y += CameraMoveSpeed * dtSeconds;
                if (currentKeyboard.IsKeyPressed(Key.Q))
                    CameraPosition.Y -= CameraMoveSpeed * dtSeconds;
            }

            if (currentGamepad != null && currentGamepad.Thumbsticks.Count != 0)
            {
                Thumbstick thumbstick = currentGamepad.Thumbsticks[0];
                if (Math.Abs(thumbstick.Position) > 0.2f)
                {
                    CameraRotationY += thumbstick.X * dtSeconds * CameraThumbstickSensitivity;
                    CameraRotationX = Math.Clamp(CameraRotationX - thumbstick.Y * dtSeconds * CameraThumbstickSensitivity, -1.57f, 1.57f);
                }

                if (currentGamepad.Thumbsticks.Count >= 2)
                {
                    thumbstick = currentGamepad.Thumbsticks[1];
                    if (Math.Abs(thumbstick.Position) > 0.2f)
                    {
                        float rot = thumbstick.Direction + CameraRotationY + MathF.PI / 2f;
                        float spd = thumbstick.Position * dtSeconds * CameraMoveSpeed;
                        CameraPosition.X += MathF.Cos(rot) * spd;
                        CameraPosition.Z += MathF.Sin(rot) * spd;
                    }
                }

                if (currentGamepad.Triggers.Count >= 2)
                {
                    if (currentGamepad.Triggers[0].Position > 0.2f)
                        CameraPosition.Y -= currentGamepad.Triggers[0].Position * dtSeconds * CameraMoveSpeed;
                    if (currentGamepad.Triggers[1].Position > 0.2f)
                        CameraPosition.Y += currentGamepad.Triggers[1].Position * dtSeconds * CameraMoveSpeed;
                }
            }
        }

        public Vector3 CalculateForwardVector()
        {
            float cosX = MathF.Cos(CameraRotationX);
            return new Vector3(MathF.Cos(CameraRotationY) * cosX, MathF.Sin(CameraRotationX), MathF.Sin(CameraRotationY) * cosX);
        }

        public Vector3 CalculateForwardVectorNoY()
        {
            return new Vector3(MathF.Cos(CameraRotationY), 0, MathF.Sin(CameraRotationY));
        }

        public Matrix4x4 CalculateViewMatrix()
        {
            //return Matrix4x4.CreateLookAt(CameraPosition, CameraPosition + CalculateForwardVector(), Vector3.UnitY);
            return Matrix4x4.CreateTranslation(-CameraPosition) * Matrix4x4.CreateRotationY(CameraRotationY + MathF.PI / 2f) * Matrix4x4.CreateRotationX(-CameraRotationX);
        }

        public Matrix4x4 CalculateViewMatrixNoTranslation()
        {
            return Matrix4x4.CreateRotationY(CameraRotationY + MathF.PI / 2f) * Matrix4x4.CreateRotationX(-CameraRotationX);
        }

        private void OnInputContextConnectionChanged(IInputDevice device, bool status)
        {
            if (device is IKeyboard)
            {
                currentKeyboard = InputContext.Keyboards.Count == 0 ? null : InputContext.Keyboards[0];
            }
            else if (device is IMouse)
            {
                currentMouse = InputContext.Mice.Count == 0 ? null : InputContext.Mice[0];
                if (currentMouse != null)
                    lastMousePos = currentMouse.Position;
            }
            else if (device is IGamepad)
            {
                currentGamepad = InputContext.Gamepads.Count == 0 ? null : InputContext.Gamepads[0];
            }
        }
    }
}
