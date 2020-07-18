using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL
{
    /// <summary>
    /// Provides functions to load OBJ files from streams into arrays of vertices.
    /// </summary>
    public static class OBJLoader
    {
        /// <summary>This character indicates that the rest of the line is a comment.</summary>
        private const char LineCommentChar = '#';

        /// <summary>This character indicates the end of the current line.</summary>
        private const char NewlineIndicator = '\n';

        /// <summary>This character is used to separate different indices when specifying a vertex.</summary>
        private const char IndicesSeparator = '/';

        /// <summary>
        /// All sequential white spaces will be transformed into a single one of this character while reading
        /// the file. This is done in <see cref="ReadNextLine(StreamReader, Span{char}, out ReadOnlySpan{char})"/>.
        /// </summary>
        private const char WhiteSpace = ' ';

        /// <summary>The lenght of the buffer where lines from a file will be read to.</summary>
        private const int LineBufferLength = 128;

        // These lists can be used and reused by the obj loading functions so they don't have to
        // instantiate and then discard a new list each time they're called.
        // Holding a reference to huge lists that might no longer be needed isn't a good idea, so
        // instead we'll hold WeakReference-s to them, allowing them to be garbage collected.
        private static WeakReference<List<int>> indicesReference;
        private static WeakReference<List<Vector3>> positionsReference;
        private static WeakReference<List<Vector3>> normalsReference;
        private static WeakReference<List<Color4b>> colorsReference;
        private static WeakReference<List<Vector2>> texCoordsReference;

        /// <summary>
        /// Loads a 3D model as an array of vertices from an OBJ file.
        /// </summary>
        /// <typeparam name="T">The type of vertex to load. This defines which vertex data will and will not be loaded.</typeparam>
        /// <param name="file">The path to the OBJ file on disk.</param>
        public static T[] FromFile<T>(string file) where T : unmanaged
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream<T>(new StreamReader(fs));
        }

        /// <summary>
        /// Loads a 3D model as an array of vertices from an OBJ file.
        /// </summary>
        /// <typeparam name="T">The type of vertex to load. This defines which vertex data will and will not be loaded.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> from which the OBJ file will be read from.</param>
        public static T[] FromStream<T>(Stream stream) where T : unmanaged
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return FromStream<T>(new StreamReader(stream));
        }

        /// <summary>
        /// Loads a 3D model as an array of vertices from an OBJ file.
        /// </summary>
        /// <typeparam name="T">The type of vertex to load. This defines which vertex data will and will not be loaded.</typeparam>
        /// <param name="reader">The <see cref="StreamReader"/> from which the OBJ file will be read from.</param>
        public static T[] FromStream<T>(StreamReader reader) where T : unmanaged
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            T tmp = default;

            // We check which vertex attributes to load and which not to depending on the vertex type.
            bool loadNormals = tmp is VertexNormal || tmp is VertexNormalColor || tmp is VertexNormalTexture || tmp is VertexNormalColorTexture;
            bool loadColors = tmp is VertexColor || tmp is VertexColorTexture || tmp is VertexNormalColor || tmp is VertexNormalColorTexture;
            bool loadTexCoords = tmp is VertexTexture || tmp is VertexColorTexture || tmp is VertexNormalTexture || tmp is VertexNormalColorTexture;

            // Let's ensure the vertex type is valid.
            if (!loadNormals && !loadColors && !loadTexCoords && !(tmp is VertexPosition || tmp is Vector3))
                throw new ObjLoaderException("Vertex format not supported. Use a library-provided format instead.");

            // We're gonna need lists to store temporary data. Let's get them form here.
            GetLists(loadNormals, loadColors, loadTexCoords, out List<Vector3> positions, out List<Vector3> normals,
                out List<Color4b> colors, out List<Vector2> texCoords, out List<int> indices);

            // When something goes wrong, it's gonna be nice to have the line number on the exception's message ;)
            int currentLineNumber = 0;

            try
            {
                // We will count the amount of vertices 
                int vertexCount = 0;

                // Lines read from the file will be held inside this buffer (if they fit)
                Span<char> lineBuffer = stackalloc char[LineBufferLength];

                while (!reader.EndOfStream)
                {
                    // Reads the next line. If the line is less than 3 characters, we ignore it.
                    ReadOnlySpan<char> line = ReadNextLine(reader, lineBuffer);
                    currentLineNumber++;
                    if (line.Length < 3)
                        continue;

                    // Let's check and process the current line then
                    if (line[0] == 'v')
                    {
                        if (line[1] == WhiteSpace)
                        {
                            // This line is declaring a vertex position (and maybe color) so
                            // let's parse three floats and add those to the positions list.
                            int index = 2;
                            Vector3 pos = new Vector3(ReadNextFloat(line, ref index), ReadNextFloat(line, ref index), ReadNextFloat(line, ref index));
                            positions.Add(pos);

                            // And if we're loading colors, then we also parse that and add it to the colors list.
                            if (loadColors)
                            {
                                Color4b col = new Color4b(ReadNextFloat(line, ref index), ReadNextFloat(line, ref index), ReadNextFloat(line, ref index));
                                colors.Add(col);
                            }
                        }
                        else if (loadNormals && line[1] == 'n' && line[2] == WhiteSpace)
                        {
                            // This line is declaring normals and we have loading normals enabled.
                            // We parse three floats and add them to the normals list.
                            int index = 3;
                            Vector3 norm = new Vector3(ReadNextFloat(line, ref index), ReadNextFloat(line, ref index), ReadNextFloat(line, ref index));
                            normals.Add(norm);
                        }
                        else if (loadTexCoords && line[1] == 't' && line[2] == WhiteSpace)
                        {
                            // This line is declaring texcoords and we have loading texcoords enabled.
                            // We parse two floats and add them to the texcoords list.
                            int index = 3;
                            Vector2 coords = new Vector2(ReadNextFloat(line, ref index), ReadNextFloat(line, ref index));
                            texCoords.Add(coords);
                        }
                    }
                    else if (line[0] == 'f' && line[1] == WhiteSpace)
                    {
                        // This line is declaring a face.
                        int index = 2;

                        // We get the indices for three vertices and save them ordered on the indices list.
                        for (int i = 0; i < 3; i++)
                        {
                            ReadThreeIntegers(line, ref index, out int first, out int second, out int third);
                            indices.Add(first - 1);
                            if (loadNormals) indices.Add(third - 1);
                            if (loadTexCoords) indices.Add(second - 1);
                            vertexCount++;

                            // We need to substract 1 to the indices because the indexing on the obj file
                            // starts at 1, while our lists start at 0.
                        }
                    }
                }

                // We're done reading the file. We now need to process the data.
                T[] vertices = new T[vertexCount];

                // Time to get funky
                unsafe
                {
                    int ind = 0;
                    for (int i = 0; i < vertexCount; i++)
                    {
                        // We don't know what type of vertex we have, but we know the attributes
                        // are always in the same order: POSITION NORMAL COLOR TEXCOORDS
                        // So the easiest way to set the data of whatever a T is, is to just
                        // grab a pointer and manually set it, since we can't do new T(position, etc).

                        // The data will be set to tmp, which has a fixed address (it's in the stack)
                        float* ptrToTmp = (float*)&tmp;

                        // We write the position onto tmp and advance the pointer
                        int posIndex = indices[ind++];
                        Vector3 pos = positions[posIndex];
                        ptrToTmp[0] = pos.X;
                        ptrToTmp[1] = pos.Y;
                        ptrToTmp[2] = pos.Z;
                        ptrToTmp += 3;

                        // If the vertex type has normal, we write the normal and advance the pointer
                        if (loadNormals)
                        {
                            pos = normals[indices[ind++]];
                            ptrToTmp[0] = pos.X;
                            ptrToTmp[1] = pos.Y;
                            ptrToTmp[2] = pos.Z;
                            ptrToTmp += 3;
                        }

                        // If the vertex type has color, we write the color and advance the pointer
                        if (loadColors)
                        {
                            ((Color4b*)ptrToTmp)[0] = colors[posIndex];
                            ptrToTmp++;
                        }

                        // If the vertex type has texcoords, we write the texcoords and advance the pointer
                        if (loadTexCoords)
                        {
                            Vector2 tex = texCoords[indices[ind++]];
                            ptrToTmp[0] = tex.X;
                            ptrToTmp[1] = 1 - tex.Y;
                            ptrToTmp += 2;
                        }

                        // Now T has the value we wanted. We can store it in the array.
                        vertices[i] = tmp;

                        // Yes, we could write directly to the array. But we'd have to fix it in place.
                    }
                }

                // Done!
                return vertices;
            }
            catch (Exception e)
            {
                // If the error happened before the reader reached end of stream, it was an error
                // in a specific line. Otherwise, it was an error constructing the vertex data.
                if (reader.EndOfStream)
                    throw new ObjLoaderException("Error processing obj data. Ensure all indices are valid?", e);
                else
                    throw new ObjLoaderException("Error in line " + currentLineNumber, e);
            }
            finally
            {
                // We're done here. Let's return the lists so they can be reused if needed.
                ReturnLists(positions, normals, colors, texCoords, indices);
            }
        }

        /// <summary>
        /// Advances index until it is pointing at a char in the <see cref="ReadOnlySpan{T}"/> that is not a whitespace.
        /// </summary>
        private static void SkipWhitespaces(ReadOnlySpan<char> chars, ref int index)
        {
            while (char.IsWhiteSpace(chars[index]))
                index++;
        }

        /// <summary>
        /// Returns the index of the first occurrance of a a whitespace character in the chars
        /// <see cref="ReadOnlySpan{T}"/>, starting at startIndex.
        /// </summary>
        private static int FindNextWhitespace(ReadOnlySpan<char> chars, int startIndex)
        {
            while (startIndex < chars.Length && !char.IsWhiteSpace(chars[startIndex]))
                startIndex++;
            return startIndex;
        }

        /// <summary>
        /// Advances index until it is pointing to a position in the chars <see cref="ReadOnlySpan{T}"/>
        /// that contains the requested character.
        /// </summary>
        private static void AdvanceUntilNext(ReadOnlySpan<char> chars, ref int index, char character)
        {
            while (index < chars.Length && chars[index] != character)
                index++;
        }

        /// <summary>
        /// Reads and parses a float positioned in between whitespaces from the string and
        /// advances index to the next character after the float's last digit.
        /// </summary>
        private static float ReadNextFloat(ReadOnlySpan<char> chars, ref int index)
        {
            SkipWhitespaces(chars, ref index);
            int startIndex = index;
            index = FindNextWhitespace(chars, index);
            return float.Parse(chars[startIndex..index], NumberStyles.AllowThousands | NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Reads three integers separated by <see cref="IndicesSeparator"/> and advances index
        /// to the next character after the integers.
        /// </summary>
        private static void ReadThreeIntegers(ReadOnlySpan<char> chars, ref int index, out int first, out int second, out int third)
        {
            SkipWhitespaces(chars, ref index);
            int start = index;
            AdvanceUntilNext(chars, ref index, IndicesSeparator);
            if (!int.TryParse(chars[start..index], NumberStyles.Integer, CultureInfo.InvariantCulture, out first))
                first = -1;
            start = ++index;

            AdvanceUntilNext(chars, ref index, IndicesSeparator);
            if (start == index || !int.TryParse(chars[start..index], NumberStyles.Integer, CultureInfo.InvariantCulture, out second))
                second = -1;
            start = ++index;

            index = FindNextWhitespace(chars, index);
            if (start == index || !int.TryParse(chars[start..index], NumberStyles.Integer, CultureInfo.InvariantCulture, out third))
                third = -1;
            index++;
        }

        /// <summary>
        /// Reads the next line from the reader and returns the characters as a <see cref="ReadOnlySpan{T}"/>.
        /// The function will try to use lineBuffer to store the characters, but if there are too many
        /// then it will allocate a string.
        /// This will also trim off comments and remove any excess whitespaces.
        /// </summary>
        /// <param name="reader">The <see cref="StreamReader"/> to read the chars from.</param>
        /// <param name="lineBuffer">The preferred buffer on which to store the characters read.</param>
        private static ReadOnlySpan<char> ReadNextLine(StreamReader reader, Span<char> lineBuffer)
        {
            // The index on lineBuffer where we're currently writting.
            int index = 0;

            char lastCharWritten = default;
            char currentChar;
            while (!reader.EndOfStream)
            {
                // We read the next character from the stream.
                currentChar = (char)reader.Read();

                // If the current char is a newline, we've reached the end of this line.
                if (currentChar == NewlineIndicator)
                    break;

                // If we found a comment, we'll cut the line up to there and skip the rest.
                if (currentChar == LineCommentChar)
                {
                    SkipUntilNextLine(reader);
                    break;
                }

                // Note: '\n' and '\r' characters also count as whitespace!
                if (char.IsWhiteSpace(currentChar))
                {
                    // If a line starts with white characters, we ignore those.
                    // If the previous character was a white space, then we ignore extra consecutive white spaces.
                    if (index == 0 || lastCharWritten == WhiteSpace)
                        continue;

                    // This way, only the first whitespace character in a sequence will be taken.
                    // Also, no matter what type of whitespace it is, we turn it into the WhiteSpace char
                    currentChar = WhiteSpace;

                    // Therefore, a sequence like two tabs followed by one space followed by three tabs
                    // would all be converted into a single WhiteSpace while reading them.
                }

                // If we need one more character but lineBuffer is filled, then we'll just read the
                // rest of the line as a string directly from the reader and concat everything.
                if (index == lineBuffer.Length)
                {
                    // This might not look all that conventional, but seeking a StreamReader
                    // is a bit of a pain in the ass...
                    return ConcatLine(lineBuffer, currentChar, reader.ReadLine());
                }

                // We write the char to the lineBuffer and remember it as the last char we wrote.
                lineBuffer[index++] = currentChar;
                lastCharWritten = currentChar;
            }

            // EndOfStream reached. We have less than lineBuffer.Lenght characters.
            // We set line to the subset of the linesBuffer and trim off excess whitespaces.
            ReadOnlySpan<char> line = lineBuffer.Slice(0, index);
            return line.TrimEnd();
        }

        /// <summary>
        /// Reads (and discarts) characters from the stream until a '\n' is found.
        /// Returns whether the end of the file was reached.
        /// </summary>
        /// <param name="reader">The <see cref="StreamReader"/> to read the chars from.</param>
        private static void SkipUntilNextLine(StreamReader reader)
        {
            while (!reader.EndOfStream)
                if (reader.Read() == NewlineIndicator)
                    return;
        }

        /// <summary>
        /// Concatenates a given <see cref="ReadOnlySpan{T}"/>, a char and a string
        /// (in that order) into a single heap-allocated <see cref="ReadOnlySpan{T}"/>.
        /// This will also trim out comments and remove excess whitespaces.
        /// </summary>
        private static ReadOnlySpan<char> ConcatLine(ReadOnlySpan<char> lineBuffer, char currentChar, string line)
        {
            // We allocate enough characters for the entire line.
            // We might not need this entire buffer though.
            Span<char> buff = new char[lineBuffer.Length + 1 + line.Length];
            
            // We copy the lineBuffer to the beginning of buff.
            lineBuffer.CopyTo(buff.Slice(0, lineBuffer.Length));

            // The next character after all of that, we set it to currentChar.
            int index = lineBuffer.Length;
            buff[index++] = currentChar;

            // We'll process the rest (the characters from the line string)
            char lastWrittenChar = currentChar;
            for (int i = 0; i < line.Length; i++)
            {
                currentChar = line[i];

                // If we found a comment, the line ends here.
                if (currentChar == LineCommentChar)
                    break;

                // If there are multiple sequential whitespace characters, we just place one WhiteSpace.
                if (char.IsWhiteSpace(currentChar))
                {
                    if (lastWrittenChar == WhiteSpace)
                        continue;

                    currentChar = WhiteSpace;
                }

                buff[index++] = currentChar;
                lastWrittenChar = currentChar;
            }

            // We've copied all the relevant characters. Let's slice and trim buff and return it.
            return ((ReadOnlySpan<char>)buff.Slice(0, index)).TrimEnd();
        }

        /// <summary>
        /// Gets the requested lists. If there are previous list instances that can be reused, those are
        /// returned. Otherwise, new lists are created where necessary.
        /// </summary>
        private static void GetLists(bool loadNormals, bool loadColors, bool loadTexCoords, out List<Vector3> positions,
            out List<Vector3> normals, out List<Color4b> colors, out List<Vector2> texCoords, out List<int> indices)
        {
            if (indicesReference == null)
                indicesReference = new WeakReference<List<int>>(null);

            lock (indicesReference)
            {
                if (indicesReference.TryGetTarget(out indices))
                {
                    indicesReference.SetTarget(null);
                    indices.Clear();
                }
                else
                    indices = new List<int>(128);

                if (positionsReference != null && positionsReference.TryGetTarget(out positions))
                {
                    positionsReference.SetTarget(null);
                    positions.Clear();
                }
                else
                    positions = new List<Vector3>(128);

                if (loadNormals)
                {
                    if (normalsReference != null && normalsReference.TryGetTarget(out normals))
                    {
                        normalsReference.SetTarget(null);
                        normals.Clear();
                    }
                    else
                        normals = new List<Vector3>(128);
                }
                else
                    normals = null;

                if (loadColors)
                {
                    if (colorsReference != null && colorsReference.TryGetTarget(out colors))
                    {
                        colorsReference.SetTarget(null);
                        colors.Clear();
                    }
                    else
                        colors = new List<Color4b>(128);
                }
                else
                    colors = null;

                if (loadTexCoords)
                {
                    if (texCoordsReference != null && texCoordsReference.TryGetTarget(out texCoords))
                    {
                        texCoordsReference.SetTarget(null);
                        texCoords.Clear();
                    }
                    else
                        texCoords = new List<Vector2>(128);
                }
                else
                    texCoords = null;
            }
        }

        /// <summary>
        /// Returns the given lists so they can be reused.
        /// </summary>
        private static void ReturnLists(List<Vector3> positions, List<Vector3> normals, List<Color4b> colors,
            List<Vector2> texCoords, List<int> indices)
        {
            lock (indicesReference)
            {
                if (!indicesReference.TryGetTarget(out List<int> oldIndices) || oldIndices.Count < indices.Count)
                    indicesReference.SetTarget(indices);

                if (positionsReference == null)
                    positionsReference = new WeakReference<List<Vector3>>(positions);
                else if (!positionsReference.TryGetTarget(out List<Vector3> oldPos) || oldPos.Count < positions.Count)
                    positionsReference.SetTarget(positions);

                if (normals != null)
                {
                    if (normalsReference == null)
                        normalsReference = new WeakReference<List<Vector3>>(normals);
                    else if (!normalsReference.TryGetTarget(out List<Vector3> oldNorm) || oldNorm.Count < normals.Count)
                        normalsReference.SetTarget(normals);
                }

                if (colors != null)
                {
                    if (colorsReference == null)
                        colorsReference = new WeakReference<List<Color4b>>(colors);
                    else if (!colorsReference.TryGetTarget(out List<Color4b> oldCols) || oldCols.Count < colors.Count)
                        colorsReference.SetTarget(colors);
                }

                if (texCoords != null)
                {
                    if (texCoordsReference == null)
                        texCoordsReference = new WeakReference<List<Vector2>>(texCoords);
                    else if (!texCoordsReference.TryGetTarget(out List<Vector2> oldCoords) || oldCoords.Count < texCoords.Count)
                        texCoordsReference.SetTarget(texCoords);
                }
            }
        }
    }
}
