using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input;

namespace TrippyTestBase
{
    /// <summary>
    /// Handles basic input managing for a 3D camera.
    /// </summary>
    public sealed class InputManager3D : IDisposable
    {
        /// <summary>The <see cref="IInputContext"/> this <see cref="InputManager3D"/> uses.</summary>
        public readonly IInputContext InputContext;

        /// <summary>The keyboard this <see cref="InputManager3D"/> is currently using, or null.</summary>
        public IKeyboard CurrentKeyboard { get; private set; }
        /// <summary>The mouse this <see cref="InputManager3D"/> is currently using, or null.</summary>
        public IMouse CurrentMouse { get; private set; }
        /// <summary>The gamepad this <see cref="InputManager3D"/> is currently using, or null.</summary>
        public IGamepad CurrentGamepad { get; private set; }

        /// <summary>Whether to lock the mouse in place while moving the camera.</summary>
        public bool LockMouseWhileRotating = true;

        /// <summary>How many units of rotation to move per pixel the mouse moves.</summary>
        public float MouseSensitivity = 0.005f;

        /// <summary>How many units of rotation to move per thumbstick distance to it's origin.</summary>
        public float CameraThumbstickSensitivity = 3;

        /// <summary>The maximum speed at which the camera can move.</summary>
        public float CameraMoveSpeed = 1;

        private Vector2 lastMousePos;

        /// <summary>The camera's rotation alongside the Y axis.</summary>
        public float CameraRotationY;
        /// <summary>The camera's rotation alongside the X axis.</summary>
        public float CameraRotationX;
        /// <summary>The camera's position in world space.</summary>
        public Vector3 CameraPosition;

        /// <summary>
        /// Creates a <see cref="InputManager3D"/> with the specified <see cref="IInputContext"/>.
        /// </summary>
        public InputManager3D(IInputContext inputContext)
        {
            InputContext = inputContext ?? throw new ArgumentNullException(nameof(inputContext));

            inputContext.ConnectionChanged += OnInputContextConnectionChanged;

            CurrentKeyboard = InputContext.Keyboards.Count == 0 ? null : InputContext.Keyboards[0];
            CurrentMouse = InputContext.Mice.Count == 0 ? null : InputContext.Mice[0];
            CurrentGamepad = InputContext.Gamepads.Count == 0 ? null : InputContext.Gamepads[0];

            CameraRotationX = 0;
            CameraRotationY = 0;
            CameraPosition = Vector3.Zero;
        }

        /// <summary>
        /// Updates the values in this <see cref="InputManager3D"/> by processing input states.
        /// </summary>
        /// <param name="dtSeconds">The amount of time since the last update, measured in seconds.</param>
        public void Update(float dtSeconds)
        {
            if (CurrentMouse != null)
            {
                if (CurrentMouse.IsButtonPressed(MouseButton.Left))
                {
                    CameraRotationY += (CurrentMouse.Position.X - lastMousePos.X) * MouseSensitivity;
                    CameraRotationX = Math.Clamp(CameraRotationX + (CurrentMouse.Position.Y - lastMousePos.Y) * -MouseSensitivity, -1.57f, 1.57f);
                    if (LockMouseWhileRotating)
                        CurrentMouse.Position = lastMousePos;
                }

                lastMousePos = CurrentMouse.Position;
            }

            if (CurrentKeyboard != null)
            {
                float sinY = MathF.Sin(CameraRotationY);
                float cosY = MathF.Cos(CameraRotationY);

                if (CurrentKeyboard.IsKeyPressed(Key.W))
                    CameraPosition += new Vector3(cosY, 0, sinY) * CameraMoveSpeed * dtSeconds;
                if (CurrentKeyboard.IsKeyPressed(Key.S))
                    CameraPosition -= new Vector3(cosY, 0, sinY) * CameraMoveSpeed * dtSeconds;

                if (CurrentKeyboard.IsKeyPressed(Key.A))
                    CameraPosition += new Vector3(sinY, 0, -cosY) * CameraMoveSpeed * dtSeconds;
                if (CurrentKeyboard.IsKeyPressed(Key.D))
                    CameraPosition -= new Vector3(sinY, 0, -cosY) * CameraMoveSpeed * dtSeconds;

                if (CurrentKeyboard.IsKeyPressed(Key.E))
                    CameraPosition.Y += CameraMoveSpeed * dtSeconds;
                if (CurrentKeyboard.IsKeyPressed(Key.Q))
                    CameraPosition.Y -= CameraMoveSpeed * dtSeconds;
            }

            if (CurrentGamepad != null && CurrentGamepad.Thumbsticks.Count != 0)
            {
                Thumbstick thumbstick = CurrentGamepad.Thumbsticks[0];
                if (Math.Abs(thumbstick.Position) > 0.2f)
                {
                    CameraRotationY += thumbstick.X * dtSeconds * CameraThumbstickSensitivity;
                    CameraRotationX = Math.Clamp(CameraRotationX - thumbstick.Y * dtSeconds * CameraThumbstickSensitivity, -1.57f, 1.57f);
                }

                if (CurrentGamepad.Thumbsticks.Count >= 2)
                {
                    thumbstick = CurrentGamepad.Thumbsticks[1];
                    if (Math.Abs(thumbstick.Position) > 0.2f)
                    {
                        float rot = thumbstick.Direction + CameraRotationY + MathF.PI / 2f;
                        float spd = thumbstick.Position * dtSeconds * CameraMoveSpeed;
                        CameraPosition.X += MathF.Cos(rot) * spd;
                        CameraPosition.Z += MathF.Sin(rot) * spd;
                    }
                }

                if (CurrentGamepad.Triggers.Count >= 2)
                {
                    if (CurrentGamepad.Triggers[0].Position > 0.2f)
                        CameraPosition.Y -= CurrentGamepad.Triggers[0].Position * dtSeconds * CameraMoveSpeed;
                    if (CurrentGamepad.Triggers[1].Position > 0.2f)
                        CameraPosition.Y += CurrentGamepad.Triggers[1].Position * dtSeconds * CameraMoveSpeed;
                }
            }
        }

        /// <summary>
        /// Calculates a normalized vector that points forward based on the camera's rotation.
        /// </summary>
        public Vector3 CalculateForwardVector()
        {
            float cosX = MathF.Cos(CameraRotationX);
            return new Vector3(MathF.Cos(CameraRotationY) * cosX, MathF.Sin(CameraRotationX), MathF.Sin(CameraRotationY) * cosX);
        }

        /// <summary>
        /// Calculates a normalized vector that points forward based on the camera's Y rotation.
        /// </summary>
        /// <remarks>
        /// The returned vector will have a Y value of 0.
        /// </remarks>
        public Vector3 CalculateForwardVectorNoY()
        {
            return new Vector3(MathF.Cos(CameraRotationY), 0, MathF.Sin(CameraRotationY));
        }

        /// <summary>
        /// Calculates a view matrix based on the camera's position and rotation.
        /// </summary>
        public Matrix4x4 CalculateViewMatrix()
        {
            return Matrix4x4.CreateTranslation(-CameraPosition) * Matrix4x4.CreateRotationY(CameraRotationY + MathF.PI / 2f) * Matrix4x4.CreateRotationX(-CameraRotationX);
        }

        /// <summary>
        /// Calculates a view matrix based on the camera's rotation (ignoring position).
        /// </summary>
        public Matrix4x4 CalculateViewMatrixNoTranslation()
        {
            return Matrix4x4.CreateRotationY(CameraRotationY + MathF.PI / 2f) * Matrix4x4.CreateRotationX(-CameraRotationX);
        }

        private void OnInputContextConnectionChanged(IInputDevice device, bool status)
        {
            if (device is IKeyboard)
            {
                CurrentKeyboard = InputContext.Keyboards.Count == 0 ? null : InputContext.Keyboards[0];
            }
            else if (device is IMouse)
            {
                CurrentMouse = InputContext.Mice.Count == 0 ? null : InputContext.Mice[0];
                if (CurrentMouse != null)
                    lastMousePos = CurrentMouse.Position;
            }
            else if (device is IGamepad)
            {
                CurrentGamepad = InputContext.Gamepads.Count == 0 ? null : InputContext.Gamepads[0];
            }
        }

        public void Dispose()
        {
            InputContext.ConnectionChanged -= OnInputContextConnectionChanged;
        }
    }
}
