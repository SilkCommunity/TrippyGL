using System.Numerics;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using TrippyGL;
using TrippyTestBase;

namespace IndexedRendering
{
    // This test makes a 7 segment display using indexed rendering.
    // The number displayed can be changed by pressing a number on the keyboard.

    // All the vertices for the display are stored in a buffer and indices are used to define
    // the order in which they are taken. By using different indices, we can draw the different numbers.

    class IndexedRendering : TestBase
    {
        VertexBuffer<SimpleVertex> vertexBuffer;
        ShaderProgram shaderProgram;

        /// <summary>The location in the index buffer where the index data for each number starts.</summary>
        int[] indicesStart;
        /// <summary>The number the 7 segment display is currently rendering.</summary>
        int currentSelectedIndex = 0;

        protected override void OnLoad()
        {
            SimpleVertex[] vertexData = Indices.Vertices;

            // We create a VertexBuffer with just enough vertex storage for all the vertices
            // and enough index storage for all the indices, and give it vertexData as initial vertex data.
            vertexBuffer = new VertexBuffer<SimpleVertex>(graphicsDevice, (uint)vertexData.Length, (uint)Indices.TotalIndicesLength, DrawElementsType.UnsignedByte, BufferUsageARB.StaticDraw, vertexData);

            // We will store the location in the index subset where each number's indices start in this array
            indicesStart = new int[Indices.AllNumbersIndices.Length];

            // We will copy all the data from all the number's indices over to the index buffer subset.
            // We copy them in order (that is, Number0 followed by Number1 followed by Number2 etc)
            // and in indicesStart we store in which location of the subset the indices of each number start.
            int indexStart = 0;
            for (int i = 0; i < Indices.AllNumbersIndices.Length; i++)
            {
                indicesStart[i] = indexStart;
                vertexBuffer.IndexSubset.SetData(Indices.AllNumbersIndices[i], (uint)indexStart);
                indexStart += Indices.AllNumbersIndices[i].Length;
            }

            shaderProgram = ShaderProgram.FromFiles<SimpleVertex>(graphicsDevice, "vs.glsl", "fs.glsl", "vPosition");

            shaderProgram.Uniforms["color"].SetValueVec4(1f, 0f, 0f, 1f);

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.DrawElements(PrimitiveType.Triangles, indicesStart[currentSelectedIndex], (uint)Indices.AllNumbersIndices[currentSelectedIndex].Length);

            Window.SwapBuffers();
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int idk)
        {
            if (key >= Key.Number0 && key <= Key.Number9)
                currentSelectedIndex = key - Key.Number0;
            else if (key >= Key.Keypad0 && key <= Key.Keypad9)
                currentSelectedIndex = key - Key.Keypad0;
        }

        protected override void OnResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(size.Width / (float)size.Height * 2f, 2f, 0.1f, 10f));
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
        }
    }
}
