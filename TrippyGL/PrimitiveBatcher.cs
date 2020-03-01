using System;

namespace TrippyGL
{
    /// <summary>
    /// Batches primitives, both triangles and lines.
    /// The batcher works for both lines and triangles, just beware that both types are batched independently.
    /// The result of the batcher is always triangle/line list (GL_TRIANGLES or GL_LINES).
    /// </summary>
    /// <typeparam name="T">The type of vertex to batch.</typeparam>
    public class PrimitiveBatcher<T> where T : struct
    {
        //TODO: Optimize all add operations by making them unsafe & utilizing pointers

        private T[] triangles;
        private T[] lines;

        /// <summary>Gets a <see cref="Span{T}"/> with the batched triangle vertices.</summary>
        public Span<T> TriangleVertices => triangles.AsSpan(0, TriangleVertexCount);

        /// <summary>Gets a <see cref="Span{T}"/> with the batched line vertices.</summary>
        public Span<T> LineVertices => lines.AsSpan(0, LineVertexCount);

        /// <summary>Gets the amount of triangle vertices currently stored by the primitive batcher.</summary>
        public int TriangleVertexCount { get; private set; }

        /// <summary>Gets the amount of line vertices currently stored by the primitive batcher.</summary>
        public int LineVertexCount { get; private set; }

        /// <summary>Gets the amount of triangles currently stored by the primitive batcher.</summary>
        public int TriangleCount => TriangleVertexCount / 3;

        /// <summary>Gets the amount of lines currently stored by the primitive batcher.</summary>
        public int LineCount => LineVertexCount / 2;

        /// <summary>The amount of triangle vertices the primitive batcher can currently hold.</summary>
        public int TriangleVertexCapacity
        {
            get { return triangles.Length; }
            set
            {
                if (TriangleVertexCount > value)
                    throw new InvalidOperationException("The primitive batcher's capacity must be able to hold the currently batched vertices");
                ResizeTriangles(value);
            }
        }

        /// <summary>The amount of line vertices the primitive batcher can currently hold.</summary>
        public int LineVertexCapacity
        {
            get { return lines.Length; }
            set
            {
                if (LineVertexCount > value)
                    throw new InvalidOperationException("The primitive batcher's capacity must be able to hold the currently batched vertices");
                ResizeLines(value);
            }
        }

        /// <summary>
        /// Creates a primitive batcher with the specified initial capacities.
        /// </summary>
        /// <param name="initialTriangleCount">The initial capacity for triangles, in vertex. A negative or 0 value means you don't want to use triangles.</param>
        /// <param name="initialLineCount">The initial capacity for lines, in vertex. A negative or 0 value means you don't want to use lines.</param>
        public PrimitiveBatcher(int initialTriangleCount, int initialLineCount)
        {
            if (initialLineCount > 0)
            {
                lines = new T[initialLineCount];
                LineVertexCount = 0;
            }
            else
                LineVertexCount = -1;

            if (initialTriangleCount > 0)
            {
                triangles = new T[initialTriangleCount];
                TriangleVertexCount = 0;
            }
            else
                TriangleVertexCount = -1;
        }

        /// <summary>
        /// Creates a primitive batcher with the default capacities for both arrays (96).
        /// </summary>
        public PrimitiveBatcher() : this(96, 96)
        {

        }

        #region AddTriangles

        /// <summary>
        /// Adds a single triangle.
        /// </summary>
        /// <param name="v1">The first vertex of the triangle.</param>
        /// <param name="v2">The second vertex of the triangle.</param>
        /// <param name="v3">The third vertex of the triangle.</param>
        public void AddTriangle(T v1, T v2, T v3)
        {
            EnsureTriangleSpace(TriangleVertexCount + 3);

            triangles[TriangleVertexCount++] = v1;
            triangles[TriangleVertexCount++] = v2;
            triangles[TriangleVertexCount++] = v3;
        }

        /// <summary>
        /// Adds a list of triangles. If the amount of vertices isn't a multiple of 3, then the last vertices will be dropped to make it so.
        /// </summary>
        /// <param name="vertices">The list of triangles as vertices.</param>
        public void AddTriangles(ReadOnlySpan<T> vertices)
        {
            int count = vertices.Length / 3 * 3;
            EnsureTriangleSpace(TriangleVertexCount + count);
            vertices.Slice(0, count).CopyTo(triangles.AsSpan(TriangleVertexCount, count));
            TriangleVertexCount += count;
        }

        /// <summary>
        /// Adds a strip of triangles.
        /// </summary>
        /// <param name="vertices">The strip of triangles as vertices.</param>
        public void AddTriangleStrip(ReadOnlySpan<T> vertices)
        {
            EnsureTriangleSpace(TriangleVertexCount + (vertices.Length - 2) * 3);
            for (int i = 2; i < vertices.Length; i++)
            {
                triangles[TriangleVertexCount++] = vertices[i - 2];
                triangles[TriangleVertexCount++] = vertices[i - 1];
                triangles[TriangleVertexCount++] = vertices[i];
            }
        }

        /// <summary>
        /// Adds a fan of triangles.
        /// </summary>
        /// <param name="vertex">The triangle fan as vertices.</param>
        public void AddTriangleFan(ReadOnlySpan<T> vertex)
        {
            EnsureTriangleSpace(TriangleVertexCount + (vertex.Length - 2) * 3);
            for (int i = 2; i < vertex.Length; i++)
            {
                triangles[TriangleVertexCount++] = vertex[0];
                triangles[TriangleVertexCount++] = vertex[i - 1];
                triangles[TriangleVertexCount++] = vertex[i];
            }
        }

        /// <summary>
        /// Adds a single quad as two triangles.
        /// </summary>
        /// <param name="v1">The first vertex of the quad.</param>
        /// <param name="v2">The second vertex of the quad.</param>
        /// <param name="v3">The third vertex of the quad.</param>
        /// <param name="v4">The fourth vertex of the quad.</param>
        public void AddQuad(T v1, T v2, T v3, T v4)
        {
            EnsureTriangleSpace(TriangleVertexCount + 6);

            triangles[TriangleVertexCount++] = v1;
            triangles[TriangleVertexCount++] = v2;
            triangles[TriangleVertexCount++] = v3;

            triangles[TriangleVertexCount++] = v1;
            triangles[TriangleVertexCount++] = v3;
            triangles[TriangleVertexCount++] = v4;
        }

        /// <summary>
        /// Adds a list of quads that will be each converted into two triangles. If the amount of vertices isn't a
        /// multiple of 4, then the last vertices will be dropped to make it so.
        /// </summary>
        /// <param name="vertex">The vertices of the quads.</param>
        public void AddQuads(ReadOnlySpan<T> vertex)
        {
            int max = vertex.Length / 4 * 4;
            EnsureTriangleSpace(TriangleVertexCount + max / 4 * 6);
            for (int i = 0; i < max; i += 4)
            {
                // Writting them this way ensures the orientation of the vertex is the right
                // This way culling works properly with quads

                triangles[TriangleVertexCount++] = vertex[i];
                triangles[TriangleVertexCount++] = vertex[i + 1];
                triangles[TriangleVertexCount++] = vertex[i + 2];

                triangles[TriangleVertexCount++] = vertex[i];
                triangles[TriangleVertexCount++] = vertex[i + 2];
                triangles[TriangleVertexCount++] = vertex[i + 3];
            }
        }

        #endregion AddTriangles

        #region AddLines

        /// <summary>
        /// Adds a single line.
        /// </summary>
        /// <param name="v1">The first vertex of the line.</param>
        /// <param name="v2">The second vertex of the line.</param>
        public void AddLine(T v1, T v2)
        {
            EnsureLineSpace(LineVertexCount + 2);
            lines[LineVertexCount++] = v1;
            lines[LineVertexCount++] = v2;
        }

        /// <summary>
        /// Adds a list of lines. If the amount of vertices isn't a multiple of 2, then the last vertex is dropped to make it so.
        /// </summary>
        /// <param name="vertices">The list of lines as vertices.</param>
        public void AddLines(ReadOnlySpan<T> vertices)
        {
            int count = vertices.Length / 2 * 2;
            EnsureLineSpace(LineVertexCount + count);
            vertices.Slice(0, count).CopyTo(lines.AsSpan(LineVertexCount, count));
            LineVertexCount += count;
        }

        /// <summary>
        /// Adds a strip of lines.
        /// </summary>
        /// <param name="vertices">The strip of lines as vertices.</param>
        public void AddLineStrip(ReadOnlySpan<T> vertices)
        {
            EnsureLineSpace(LineVertexCount + (vertices.Length - 1) * 2);
            for (int i = 1; i < vertices.Length; i++)
            {
                lines[LineVertexCount++] = vertices[i - 1];
                lines[LineVertexCount++] = vertices[i];
            }
        }

        /// <summary>
        /// Adds a loop of lines.
        /// </summary>
        /// <param name="vertices">The loop of lines as vertices.</param>
        public void AddLineLoop(ReadOnlySpan<T> vertices)
        {
            EnsureLineSpace(LineVertexCount + vertices.Length * 2);
            for (int i = 1; i < vertices.Length; i++)
            {
                lines[LineVertexCount++] = vertices[i - 1];
                lines[LineVertexCount++] = vertices[i];
            }
            lines[LineVertexCount++] = vertices[vertices.Length - 1];
            lines[LineVertexCount++] = vertices[0];
        }

        #endregion AddLines

        /// <summary>
        /// Ensures the primitive batcher has enough space for a specified amount of triangle vertices.
        /// If there isn't enough space, the list will be expanded exponentially.
        /// </summary>
        /// <param name="requiredVertexCount">The minimum amount of triangle vertices required.</param>
        public void EnsureTriangleSpace(int requiredVertexCount)
        {
            if (requiredVertexCount > triangles.Length)
            {
                // Finds the smallest number that is greater than requiredVertexCount and satisfies this equation:
                // " newLength = oldLength * 2 ^ X " where X is an integer
                // I swear to god I don't know how I came up with this calculation, it literally just came to me

                const double log2 = 0.30102999566398119521373889472449;
                int power = (int)Math.Ceiling(Math.Log(requiredVertexCount) / log2 - Math.Log(triangles.Length) / log2);
                ResizeTriangles(triangles.Length * TrippyMath.IntegerPow(2, power));
            }
        }

        /// <summary>
        /// Ensures the primitive batcher has enough space for a specified amount of line vertices.
        /// If there isn't enough space, the list will be expanded exponentially.
        /// </summary>
        /// <param name="requiredVertexCount">The minimum amount of lines vertices required.</param>
        public void EnsureLineSpace(int requiredVertexCount)
        {
            if (requiredVertexCount > lines.Length)
            {
                // Finds the smallest number that is greater than requiredVertexCount and satisfies this equation:
                // " newLength = oldLength * 2 ^ X " where X is an integer

                const double log2 = 0.30102999566398119521373889472449;
                int power = (int)Math.Ceiling(Math.Log(requiredVertexCount) / log2 - Math.Log(lines.Length) / log2);
                ResizeLines(lines.Length * TrippyMath.IntegerPow(2, power));
            }
        }

        /// <summary>
        /// Resizes the triangles array to the new specified length (which is assumed to be higher than the old length).
        /// </summary>
        /// <param name="newLength">The new triangles array length. Assumed to be greater than <see cref="TriangleVertexCapacity"/>.</param>
        private void ResizeTriangles(int newLength)
        {
            T[] oldTriangles = triangles;
            triangles = new T[newLength];
            for (int i = 0; i < TriangleVertexCount; i++)
                triangles[i] = oldTriangles[i];
        }

        /// <summary>
        /// Resizes the lines array to the new specified length (which is a ssumed to be higher than the old length).
        /// </summary>
        /// <param name="newLength">The new lines array length. Assumed to be greater than <see cref="LineVertexCapacity"/>.</param>
        private void ResizeLines(int newLength)
        {
            T[] oldLines = lines;
            lines = new T[newLength];
            for (int i = 0; i < LineVertexCount; i++)
                lines[i] = oldLines[i];
        }

        /// <summary>
        /// Resizes the triangles array to make it hold exactly the current amount of triangle vertices.
        /// </summary>
        public void TrimTriangles()
        {
            if (TriangleVertexCount != triangles.Length)
                ResizeTriangles(TriangleVertexCount);
        }

        /// <summary>
        /// Resizes the lines array to make it hold exactly the current amount of line vertices.
        /// </summary>
        public void TrimLines()
        {
            if (LineVertexCount != lines.Length)
                ResizeLines(LineVertexCount);
        }

        /// <summary>
        /// Clears the list of triangles, resetting it to 0 triangles.
        /// </summary>
        public void ClearTriangles()
        {
            TriangleVertexCount = 0;
        }

        /// <summary>
        /// Clears the list of lines, resetting it to 0 lines.
        /// </summary>
        public void ClearLines()
        {
            LineVertexCount = 0;
        }

        /// <summary>
        /// Writes all the triangle vertices to the given buffer subset. The buffer subset must have enough storage for this.
        /// </summary>
        /// <param name="buffer">The buffer where the triangle vertices will be written to.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to, measured in elements.</param>
        public void WriteTrianglesTo(DataBufferSubset<T> buffer, int storageOffset = 0)
        {
            buffer.SetData(TriangleVertices, storageOffset);
        }

        /// <summary>
        /// Writes all the line vertices to the given buffer subset. The buffer subset must have enough storage for this.
        /// </summary>
        /// <param name="buffer">The buffer where the line vertices will be written to.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to, measured in elements.</param>
        public void WriteLinesTo(DataBufferSubset<T> buffer, int storageOffset = 0)
        {
            buffer.SetData(LineVertices, storageOffset);
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(TriangleVertexCount) + "=", TriangleVertexCount.ToString(),
                ", " + nameof(LineVertexCount) + "=", LineVertexCount.ToString(),
                ", " + nameof(TriangleVertexCapacity) + "=", TriangleVertexCapacity.ToString(),
                ", " + nameof(LineVertexCapacity) + "=", LineVertexCapacity.ToString()
            );
        }
    }
}
