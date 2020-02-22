using System;

namespace TrippyGL
{
    /// <summary>
    /// Batches primitives, both triangles and lines. The batcher works for both lines and triangles, just beware that both types are batched independently.
    /// The result of the batcher is always triangle/line list (GL_TRIANGLES or GL_LINES).
    /// </summary>
    /// <typeparam name="T">The type of vertex to batch</typeparam>
    public class PrimitiveBatcher<T> where T : struct, IVertex
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
        /// <param name="v2">The second vertex.</param>
        /// <param name="v3">The third vertex.</param>
        public void AddTriangle(T v1, T v2, T v3)
        {
            EnsureTriangleSpace(TriangleVertexCount + 3);

            triangles[TriangleVertexCount++] = v1;
            triangles[TriangleVertexCount++] = v2;
            triangles[TriangleVertexCount++] = v3;
        }

        /// <summary>
        /// Adds a list of triangles. If the amount of vertices isn't a multiple of 3, then the last vertices will be dropped.
        /// </summary>
        /// <param name="vertex">The list of triangles, as vertices.</param>
        public void AddTriangles(T[] vertex)
        {
            int max = vertex.Length / 3 * 3;
            EnsureTriangleSpace(TriangleVertexCount + max);
            for (int i = 0; i < max; i++)
                triangles[TriangleVertexCount++] = vertex[i];
        }

        /// <summary>
        /// Adds a list of triangles, but only using part of the given array. If the amount of vertices isn't a multiple of 3, then the last vertices will be dropped.
        /// </summary>
        /// <param name="vertex">The list of triangles, as vertices.</param>
        /// <param name="startIndex">The index of the first element to read from the array.</param>
        /// <param name="count">The amount of vertices to add.</param>
        public void AddTriangles(T[] vertex, int startIndex, int count)
        {
            //TODO: Add parameter checks!

            count = count / 3 * 3;
            EnsureTriangleSpace(TriangleVertexCount + count);
            for (int i = 0; i < count; i++)
                triangles[TriangleVertexCount++] = vertex[startIndex + i];
        }

        /// <summary>
        /// Adds a strip of triangles.
        /// </summary>
        /// <param name="vertex">The vertices representing the triangle strip.</param>
        public void AddTriangleStrip(T[] vertex)
        {
            EnsureTriangleSpace(TriangleVertexCount + (vertex.Length - 2) * 3);
            for (int i = 2; i < vertex.Length; i++)
            {
                triangles[TriangleVertexCount++] = vertex[i - 2];
                triangles[TriangleVertexCount++] = vertex[i - 1];
                triangles[TriangleVertexCount++] = vertex[i];
            }
        }

        /// <summary>
        /// Adds a strip of triangles, but only using part of the given array.
        /// </summary>
        /// <param name="vertex">The strip of triangles, as vertices.</param>
        /// <param name="startIndex">The index of the first element to read from the array.</param>
        /// <param name="count">The amount of vertices to process.</param>
        public void AddTriangleStrip(T[] vertex, int startIndex, int count)
        {
            EnsureTriangleSpace(TriangleVertexCount + (count - 2) * 3);
            for (int i = 2; i < count; i++)
            {
                triangles[TriangleVertexCount++] = vertex[i - 2 + startIndex];
                triangles[TriangleVertexCount++] = vertex[i - 1 + startIndex];
                triangles[TriangleVertexCount++] = vertex[i + startIndex];
            }
        }

        /// <summary>
        /// Adds a fan of triangles.
        /// </summary>
        /// <param name="vertex">The vertices representing the triangle fan.</param>
        public void AddTriangleFan(T[] vertex)
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
        /// Adds a fan of triangles, but only using part of the given array.
        /// </summary>
        /// <param name="vertex">The fan of triangles, as vertices.</param>
        /// <param name="startIndex">The index of the first element to read from the array.</param>
        /// <param name="count">The amount of vertices to process.</param>
        public void AddTriangleFan(T[] vertex, int startIndex, int count)
        {
            EnsureTriangleSpace(TriangleVertexCount + (vertex.Length - 2) * 3);
            for (int i = 2; i < count; i++)
            {
                triangles[TriangleVertexCount++] = vertex[startIndex];
                triangles[TriangleVertexCount++] = vertex[i - 1 + startIndex];
                triangles[TriangleVertexCount++] = vertex[i + startIndex];
            }
        }

        /// <summary>
        /// Adds a single quad (will be converted to triangles).
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
        /// Adds a list of quads (these are converted to triangles).
        /// </summary>
        /// <param name="vertex">The quads, as vertices.</param>
        public void AddQuads(T[] vertex)
        {
            int max = vertex.Length / 4 * 4;
            EnsureTriangleSpace(TriangleVertexCount + (max / 4) * 6);
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

        /// <summary>
        /// Adds a list of quads (these are converted to triangles) but only using part of the given array.
        /// </summary>
        /// <param name="vertex">The quads, as vertices.</param>
        /// <param name="startIndex">The index of the first element to read from the array.</param>
        /// <param name="count">The amount of vertices to process.</param>
        public void AddQuads(T[] vertex, int startIndex, int count)
        {
            count = count / 4 * 4;
            EnsureTriangleSpace(TriangleVertexCount + (count / 4) * 6);
            for (int i = 0; i < count; i += 4)
            {
                // Writting them this way ensures the orientation of the vertex is the right
                // This way culling works properly with quads

                triangles[TriangleVertexCount++] = vertex[i + startIndex];
                triangles[TriangleVertexCount++] = vertex[i + 1 + startIndex];
                triangles[TriangleVertexCount++] = vertex[i + 2 + startIndex];

                triangles[TriangleVertexCount++] = vertex[i + 0 + startIndex];
                triangles[TriangleVertexCount++] = vertex[i + 2 + startIndex];
                triangles[TriangleVertexCount++] = vertex[i + 3 + startIndex];
            }
        }

        #endregion AddTriangles

        #region AddLines

        /// <summary>
        /// Adds a single line.
        /// </summary>
        /// <param name="v1">The first vertex of the line.</param>
        /// <param name="v2">The second vertex.</param>
        public void AddLine(T v1, T v2)
        {
            EnsureLineSpace(LineVertexCount + 2);
            lines[LineVertexCount++] = v1;
            lines[LineVertexCount++] = v2;
        }

        /// <summary>
        /// Adds a list of lines. If the amount of vertices isn't a multiple of 2, then the last vertex is dropped.
        /// </summary>
        /// <param name="vertex">The list of lines, as vertices.</param>
        public void AddLines(T[] vertex)
        {
            int max = vertex.Length / 2 * 2;
            EnsureLineSpace(LineVertexCount + max);
            for (int i = 0; i < max; i++)
                lines[LineVertexCount++] = vertex[i];
        }

        /// <summary>
        /// Adds a list of lines, but only using part of the given array. If count isn't a multiple of 2, then the last vertex is dropped.
        /// </summary>
        /// <param name="vertex">The strip of lines, as vertices.</param>
        /// <param name="startIndex">The index of the first element to read from the array.</param>
        /// <param name="count">The amount of vertices to process.</param>
        public void AddLines(T[] vertex, int startIndex, int count)
        {
            //TODO: Add parameter checks!

            count = count / 2 * 2;
            EnsureTriangleSpace(LineVertexCount + count);
            for (int i = 0; i < count; i++)
                lines[LineVertexCount++] = vertex[startIndex + i];
        }

        /// <summary>
        /// Adds a strip of lines.
        /// </summary>
        /// <param name="vertex">The vertices representing the line strip.</param>
        public void AddLineStrip(T[] vertex)
        {
            EnsureLineSpace(LineVertexCount + (vertex.Length - 1) * 2);
            for (int i = 1; i < vertex.Length; i++)
            {
                lines[LineVertexCount++] = vertex[i - 1];
                lines[LineVertexCount++] = vertex[i];
            }
        }

        /// <summary>
        /// Adds a strip of lines, but only using part of the given array.
        /// </summary>
        /// <param name="vertex">The strip of lines, as vertices.</param>
        /// <param name="startIndex">The index of the first element to read from the array.</param>
        /// <param name="count">The amount of vertices to process.</param>
        public void AddLineStrip(T[] vertex, int startIndex, int count)
        {
            count = count / 2 * 2;
            EnsureLineSpace(LineVertexCount + (count - 2) * 3);
            for (int i = 1; i < count; i++)
            {
                lines[LineVertexCount++] = vertex[i - 1 + startIndex];
                lines[LineVertexCount++] = vertex[i + startIndex];
            }
        }

        /// <summary>
        /// Adds a loop of lines.
        /// </summary>
        /// <param name="vertex">The loop of lines, as vertices.</param>
        public void AddLineLoop(T[] vertex)
        {
            EnsureLineSpace(LineVertexCount + vertex.Length * 2);
            for (int i = 1; i < vertex.Length; i++)
            {
                lines[LineVertexCount++] = vertex[i - 1];
                lines[LineVertexCount++] = vertex[i];
            }
            lines[LineVertexCount++] = vertex[vertex.Length - 1];
            lines[LineVertexCount++] = vertex[0];
        }

        /// <summary>
        /// Adds a loop of lines, but only using part of the given array.
        /// </summary>
        /// <param name="vertex">The strip of lines, as vertices.</param>
        /// <param name="startIndex">The index of the first element to read from the array.</param>
        /// <param name="count">The amount of vertices to process.</param>
        public void AddLineLoop(T[] vertex, int startIndex, int count)
        {
            EnsureLineSpace(LineVertexCount + count * 2);
            for (int i = 1; i < count; i++)
            {
                lines[LineVertexCount++] = vertex[i - 1 + startIndex];
                lines[LineVertexCount++] = vertex[i + startIndex];
            }
            lines[LineVertexCount++] = vertex[startIndex + count - 1];
            lines[LineVertexCount++] = vertex[startIndex];
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
        /// <param name="newLength">The new triangles array length.</param>
        private void ResizeTriangles(int newLength)
        {
            // This function assumes newLength is greater than triangles.Length

            T[] oldTriangles = triangles;
            triangles = new T[newLength];
            for (int i = 0; i < TriangleVertexCount; i++)
                triangles[i] = oldTriangles[i];
        }

        /// <summary>
        /// Resizes the lines array to the new specified length (which is a ssumed to be higher than the old length).
        /// </summary>
        /// <param name="newLength">The new lines array length.</param>
        private void ResizeLines(int newLength)
        {
            // This function assumes newLength is greater than triangles.Length

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
        /// Writes all the triangles to the given buffer. If there isn't enough storage space in the buffer, an exception will be thrown.
        /// </summary>
        /// <param name="buffer">The buffer where the triangles will be written to.</param>
        public void WriteTrianglesTo(DataBufferSubset<T> buffer)
        {
            buffer.SetData(TriangleVertices);
        }

        /// <summary>
        /// Writes all the lines to the given buffer. If there isn't enough storage space in the buffer, an exception will be thrown.
        /// </summary>
        /// <param name="buffer">The buffer where the triangles will be written to.</param>
        public void WriteLinesTo(DataBufferSubset<T> buffer)
        {
            buffer.SetData(LineVertices);
        }

        // TODO: Add some ToString()?
    }
}
