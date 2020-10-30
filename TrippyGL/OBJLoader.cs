using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL
{
    /// <summary>
    /// Provides functions to load OBJ files from streams into arrays of vertices.
    /// </summary>
    public static class OBJLoader
    {
        /// <summary>This character indicates the end of the current line.</summary>
        private const char NewlineIndicator = '\n';

        /// <summary>This character is used to separate different indices when specifying a vertex.</summary>
        private const char IndicesSeparator = '/';

        /// <summary>This character is used to indicate the start of a comment that goes until the end of the line.</summary>
        private const char CommentIndicator = '#';

        /// <summary>The length of a char buffer used to parse numbers.</summary>
        private const int CharBufferLength = 32;

        /// <summary>
        /// The maximum allowed length in characters for a number. This will limit the maximum
        /// size of the char buffer from which numbers are parsed.
        /// </summary>
        private const int MaxNumberCharacterLength = 128;

        // These lists can be used and reused by the obj loading functions so they don't have to
        // instantiate and then discard a new list each time they're called.
        // Holding a reference to huge lists that might no longer be needed isn't a good idea, so
        // instead we'll hold WeakReference-s to them, allowing them to be garbage collected.
        private static readonly object listsLock = new object();
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
        /// <param name="options">Specifies options that modify how an OBJ file is parsed.</param>
        /// <returns>An array with the parsed vertex data as a triangle list.</returns>
        /// <exception cref="ObjLoaderException"/>
        public static T[] FromFile<T>(string file, ObjLoadOptions options = ObjLoadOptions.None) where T : unmanaged
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream<T>(new StreamReader(fs), options);
        }

        /// <summary>
        /// Loads a 3D model as an array of vertices from an OBJ file.
        /// </summary>
        /// <typeparam name="T">The type of vertex to load. This defines which vertex data will and will not be loaded.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> from which the OBJ file will be read from.</param>
        /// <param name="options">Specifies options that modify how an OBJ file is parsed.</param>
        /// <returns>An array with the parsed vertex data as a triangle list.</returns>
        /// <exception cref="ObjLoaderException"/>
        public static T[] FromStream<T>(Stream stream, ObjLoadOptions options = ObjLoadOptions.None) where T : unmanaged
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return FromStream<T>(new StreamReader(stream), options);
        }

        /// <summary>
        /// Loads a 3D model as an array of vertices from an OBJ file.
        /// </summary>
        /// <typeparam name="T">The type of vertex to load. This defines which vertex data will and will not be loaded.</typeparam>
        /// <param name="streamReader">The <see cref="StreamReader"/> from which the OBJ file will be read from.</param>
        /// <param name="options">Specifies options that modify how an OBJ file is parsed.</param>
        /// <returns>An array with the parsed vertex data as a triangle list.</returns>
        /// <exception cref="ObjLoaderException"/>
        public static T[] FromStream<T>(StreamReader streamReader, ObjLoadOptions options = ObjLoadOptions.None) where T : unmanaged
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            // tmp will be used to check which vertex data to load with the is pattern at the beginning
            // and to format the vertex attributes into memory after the file has been parsed.
            T tmp = default;

            // We check which vertex attributes to load and which not to depending on the vertex type.
            bool loadNormals = tmp is VertexNormal || tmp is VertexNormalColor || tmp is VertexNormalTexture || tmp is VertexNormalColorTexture;
            bool loadColors = tmp is VertexColor || tmp is VertexColorTexture || tmp is VertexNormalColor || tmp is VertexNormalColorTexture;
            bool loadTexCoords = tmp is VertexTexture || tmp is VertexColorTexture || tmp is VertexNormalTexture || tmp is VertexNormalColorTexture;

            // Let's ensure the vertex type is valid.
            if (!loadNormals && !loadColors && !loadTexCoords && !(tmp is VertexPosition || tmp is Vector3))
                throw new ObjLoaderException("Vertex format not supported. Use a library-provided vertex type instead.");

            bool largePolygons = !options.HasFlag(ObjLoadOptions.TrianglesOnly);

            // We're gonna need lists to store temporary data. Let's get them form here.
            GetLists(loadNormals, loadColors, loadTexCoords, out List<Vector3> positions, out List<Vector3> normals,
                out List<Color4b> colors, out List<Vector2> texCoords, out List<int> indices);

            // When something goes wrong, it's gonna be nice to have the line number on the exception's message ;)
            int currentLineNumber = 0;
            bool doneParsingData = false;

            try
            {
                // In this variable we will count the amount of vertices 
                int vertexCount = 0;

                // This is the buffer we'll use to parse numbers. The initial buffer is a relatively
                // small, stackallocated array but if we need more space a char[] will be allocated
                // and this span will be replaced to point at that larger array instead.
                Span<char> charBuffer = stackalloc char[CharBufferLength];

                while (!streamReader.EndOfStream)
                {
                    currentLineNumber++;

                    // This will skip empty lines (or lines that are only whitespaces) and advance
                    // the reader to skip any whitespaces at the beginning of a line.
                    if (SkipWhitespaces(streamReader))
                    {
                        SkipLine(streamReader);
                        continue;
                    }

                    // We read the next character (it's guaranteed not to be a whitespace).
                    char currentChar = (char)streamReader.Read();
                    if (currentChar == 'v')
                    {
                        // The line starts with 'v'. Let's read the next character to see what to do next.
                        currentChar = (char)streamReader.Read();

                        // The entire line might be just "v\n". This should throw an error.
                        if (streamReader.EndOfStream || currentChar == NewlineIndicator)
                            throw new FormatException("Unexpected end of line");

                        // We check that character and 
                        if (char.IsWhiteSpace(currentChar))
                        {
                            // We need to load a Vector3 position
                            positions.Add(new Vector3(
                                ReadNextFloat(streamReader, ref charBuffer),
                                ReadNextFloat(streamReader, ref charBuffer),
                                ReadNextFloat(streamReader, ref charBuffer)
                            ));

                            if (loadColors)
                            {
                                // If we're also loading colors, we load a color4b from 3 floats
                                colors.Add(new Color4b(
                                    ReadNextFloat(streamReader, ref charBuffer),
                                    ReadNextFloat(streamReader, ref charBuffer),
                                    ReadNextFloat(streamReader, ref charBuffer)
                                ));
                            }
                        }
                        else if (loadNormals && currentChar == 'n')
                        {
                            // We need to load a Vector3 normal
                            Vector3 norm = new Vector3(
                                ReadNextFloat(streamReader, ref charBuffer),
                                ReadNextFloat(streamReader, ref charBuffer),
                                ReadNextFloat(streamReader, ref charBuffer)
                            );
                            normals.Add(norm);
                        }
                        else if (loadTexCoords && currentChar == 't')
                        {
                            // We need to load a Vector2 with texture coordinates
                            Vector2 coords = new Vector2(
                                ReadNextFloat(streamReader, ref charBuffer),
                                ReadNextFloat(streamReader, ref charBuffer)
                            );
                            texCoords.Add(coords);
                        }
                    }
                    else if (currentChar == 'f')
                    {
                        // This line is specifying a face. We need to read a polygon face.
                        // The way we load faces into triangles is by making a triangle fan.
                        // Example: vertices 0 1 2 3 4 5 will be turned into the triangles:
                        // (0 1 2) (0 2 3) (0 3 4) (0 4 5)
                        // Looking at this you can see that for each triangle, we need to know
                        // the polygon's first vertex, and the last vertex of the previous triangle.

                        // We start by loading the first two vertices and verifying their indices.
                        // The indices of the first vertex will be in "first0", "second0" and "third0".
                        ReadThreeIntegers(streamReader, ref charBuffer, out int first0, out int second0, out int third0);
                        VerifyIndices(first0, second0, third0);
                        ReadThreeIntegers(streamReader, ref charBuffer, out int lastFirst, out int lastSecond, out int lastThird);
                        VerifyIndices(lastFirst, lastSecond, lastThird);

                        // When we add the indices to the list we need to substract one, because OBJ indices
                        // start at 1 (and array indices start at 0, fuck all languages where it doesn't).
                        first0--;
                        second0--;
                        third0--;
                        lastFirst--;
                        lastSecond--;
                        lastThird--;

                        // We now load the third vertex
                        ReadThreeIntegers(streamReader, ref charBuffer, out int first, out int second, out int third);

                        do
                        {
                            // We have to add one more triangle. Let's start by verifying the last indices we read.
                            VerifyIndices(first, second, third);

                            // We add the polygon's first vertex.
                            indices.Add(first0);
                            if (loadNormals) indices.Add(third0);
                            if (loadTexCoords) indices.Add(second0);

                            // We add the last vertex (taken from the previous triangle)
                            indices.Add(lastFirst);
                            if (loadNormals) indices.Add(lastThird);
                            if (loadTexCoords) indices.Add(lastSecond);

                            first--;
                            second--;
                            third--;

                            // We add the current vertex and increment vertexCount by 3.
                            indices.Add(first);
                            if (loadNormals) indices.Add(third);
                            if (loadTexCoords) indices.Add(second);

                            vertexCount += 3;

                            // The next triangle will need to know the last vertex of the previous triangle.
                            lastFirst = first;
                            lastSecond = second;
                            lastThird = third;

                            // If largePolygons is disabled, this process ends after only one triangle.
                            // Otherwise, this process continues while there are more vertices in this line.
                        } while (largePolygons && TryReadThreeIntegers(streamReader, ref charBuffer, out first, out second, out third));
                    }

                    // The entire while cycle ensures the end of the line isn't reached until now.
                    // This ensures our currentLineNumber is counting correctly.
                    SkipLine(streamReader);
                }

                // We're done reading the file. We now need to process the data.
                doneParsingData = true;
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
                // If the error happened before doneParsingData was set to true, it was an error
                // in a specific line. Otherwise, it was an error constructing the vertex data.
                if (doneParsingData)
                    throw new ObjLoaderException("Error processing obj data.", e);
                else
                    throw new ObjLoaderException("Error in line " + currentLineNumber + ": " + e.Message, e);
            }
            finally
            {
                // We're done here. Let's return the lists so they can be reused if needed.
                ReturnLists(positions, normals, colors, texCoords, indices);
            }

            // Verifies that the given indices read from a group of three integers are valid.
            void VerifyIndices(int first, int second, int third)
            {
                // We check that the position index is valid.
                if (first < 1 || first > positions.Count)
                    throw new FormatException("Invalid position index integer: " + first.ToString());

                // If we're loading normals, we check that the normal index is valid.
                if (loadNormals && (third < 1 || third > normals.Count))
                    throw new FormatException("Invalid normal index integer: " + third.ToString());

                // If we're loading texcoords, we check that the texcoord index is valid.
                if (loadTexCoords && (second < 1 || second > texCoords.Count))
                    throw new FormatException("Invalid texture coordinates index integer: " + second.ToString());
            }
        }

        /// <summary>
        /// Advances the <see cref="StreamReader"/> until a newline character (or an end of stream) is found.
        /// The stream's position is left pointing at the first character of the following line.
        /// </summary>
        private static void SkipLine(StreamReader streamReader)
        {
            while (!streamReader.EndOfStream && streamReader.Read() != NewlineIndicator) ;
        }

        /// <summary>
        /// Advances the stream's position until the next character to be returned by <see cref="StreamReader.Read()"/>
        /// is not a whitespace character, but goes no further than the end of the line.
        /// </summary>
        /// <returns>Whether the end of the line (or stream) was reached.</returns>
        /// <remarks>
        /// In case a newline character is reached, the next char to be returned by
        /// <see cref="StreamReader.Read()"/> will be a newline character.
        /// </remarks>
        private static bool SkipWhitespaces(StreamReader streamReader)
        {
            while (!streamReader.EndOfStream)
            {
                char c = (char)streamReader.Peek();

                if (c == NewlineIndicator || c == CommentIndicator)
                    return true;

                if (!char.IsWhiteSpace(c))
                    return false;

                streamReader.Read();
            }

            return true;
        }

        /// <summary>
        /// Advances the stream's position until the next character to be returned by <see cref="StreamReader.Read()"/>
        /// is not a whitespace character. If an end of line is reached, an exception is thrown.
        /// </summary>
        /// <exception cref="InvalidDataException"/>
        private static void SkipWhitespacesNoEOL(StreamReader streamReader)
        {
            if (SkipWhitespaces(streamReader))
                throw new InvalidDataException("Unexpected end of line");
        }

        /// <summary>
        /// Advances the stream's position until the next character to be returned by <see cref="StreamReader.Read()"/>
        /// is a whitespace character, or the end of the stream is reached.
        /// </summary>
        private static void SkipUntilWhitespace(StreamReader streamReader)
        {
            while (!streamReader.EndOfStream && !char.IsWhiteSpace((char)streamReader.Peek()))
                streamReader.Read();
        }

        /// <summary>
        /// Parses a float separated by whitespaces from the <see cref="StreamReader"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="StreamReader"/>'s position is left so that the last character read
        /// was the last digit of the float, so the next <see cref="StreamReader.Read()"/> there will
        /// either be a newline character or an end of stream.
        /// </remarks>
        /// <param name="streamReader">The <see cref="StreamReader"/> to read a float from.</param>
        /// <param name="charBuffer">The buffer to use to store the characters for parsing.</param>
        /// <returns>The parsed float.</returns>
        private static float ReadNextFloat(StreamReader streamReader, ref Span<char> charBuffer)
        {
            // First we skip whitespaces to look for the first digit
            SkipWhitespacesNoEOL(streamReader);

            int index = 0;
            char currentChar;

            // We go through the stream copying characters onto charBuffer until
            // we hit a whitespace.
            do
            {
                // We fetch the next character from the stream
                currentChar = (char)streamReader.Read();

                // If we don't have room for one more char in the buffer, we expand the buffer
                if (index == charBuffer.Length)
                    ExpandCharBuffer(ref charBuffer);

                // We add the character to our parsing buffer
                charBuffer[index++] = currentChar;

                // If the end of the stream was reached, we exit the loop before fetching more chars
                if (streamReader.EndOfStream)
                    break;

                // We peek at the next character. If it's not a whitespace, the loop continues.
                currentChar = (char)streamReader.Peek();
            } while (!char.IsWhiteSpace(currentChar));

            // We parse the characters into a float and return it.
            if (float.TryParse(charBuffer.Slice(0, index), NumberStyles.AllowThousands | NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;
            throw new FormatException("Invalid float format: \"" + charBuffer.Slice(0, index).ToString() + "\"");
        }

        /// <summary>
        /// Parses a group of three integers in between whitespaces, with each integer separated by
        /// <see cref="IndicesSeparator"/> characters. Missing values will be set to -1.
        /// </summary>
        private static void ReadThreeIntegers(StreamReader streamReader, ref Span<char> charBuffer, out int first, out int second, out int third)
        {
            // First, let's skip any whitespaces before the integers group.
            SkipWhitespacesNoEOL(streamReader);

            // We read the first integer. If there are no more in the group, we return here.
            if (ParseNextInt(streamReader, ref charBuffer, out first))
            {
                second = -1;
                third = -1;
                return;
            }

            // We do the same with reading the second integer.
            if (ParseNextInt(streamReader, ref charBuffer, out second))
            {
                third = -1;
                return;
            }

            // We read the third integer and skip characters until the next whitespace,
            // so any following read integers operation starts looking in the correct place.
            ParseNextInt(streamReader, ref charBuffer, out third);
            SkipUntilWhitespace(streamReader);
        }

        /// <summary>
        /// Parses a group of three integers in between whitespaces, with each integer separated by
        /// <see cref="IndicesSeparator"/> characters. Missing values will be set to -1.
        /// </summary>
        /// <returns>True if integers were found, or false if the end of line was found.</returns>
        private static bool TryReadThreeIntegers(StreamReader streamReader, ref Span<char> charBuffer, out int first, out int second, out int third)
        {
            // First, let's skip any whitespaces before the integers group. If the end of the line
            // was reached, we return false to indicate so.
            if (SkipWhitespaces(streamReader))
            {
                first = -1;
                second = -1;
                third = -1;
                return false;
            }

            // We read the first integer, If there's no more integers in the group, we return true.
            if (ParseNextInt(streamReader, ref charBuffer, out first))
            {
                second = -1;
                third = -1;
                return true;
            }

            // Now we do the same with reading the second integer.
            if (ParseNextInt(streamReader, ref charBuffer, out second))
            {
                third = -1;
                return true;
            }

            // We read the third integer and skip characters until the next whitespace,
            // so any following read integers operation starts looking in the correct place.
            ParseNextInt(streamReader, ref charBuffer, out third);
            SkipUntilWhitespace(streamReader);
            return true;
        }

        /// <summary>
        /// Parses a single int from the group of three. The parsed integer is set to the value param
        /// and the function. Empty strings will be turned into the integer -1.
        /// </summary>
        /// <returns>Whether the end of the line/stream was reached.</returns>
        static bool ParseNextInt(StreamReader streamReader, ref Span<char> charBuffer, out int value)
        {
            int index = 0;
            char currentChar = (char)streamReader.Peek();
            while (!char.IsWhiteSpace(currentChar) && currentChar != CommentIndicator)
            {
                streamReader.Read();
                if (currentChar == IndicesSeparator)
                    break;

                if (index == charBuffer.Length)
                    ExpandCharBuffer(ref charBuffer);
                charBuffer[index++] = currentChar;

                if (streamReader.EndOfStream)
                    break;

                currentChar = (char)streamReader.Peek();
            }

            if (index == 0)
                value = -1;
            else
            {
                if (!int.TryParse(charBuffer.Slice(0, index), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    throw new FormatException("Invalid int format: \"" + charBuffer.Slice(0, index).ToString() + "\"");
            }

            return streamReader.EndOfStream || currentChar == NewlineIndicator || currentChar == CommentIndicator;
        }

        /// <summary>
        /// Expands the size of a char <see cref="Span{char}"/> by allocating a larger char[]
        /// and copying all the data over to the new buffer.
        /// </summary>
        /// <param name="charBuffer">The buffer to expand.</param>
        private static void ExpandCharBuffer(ref Span<char> charBuffer)
        {
            if (charBuffer.Length >= MaxNumberCharacterLength)
                throw new FormatException("Numbers be less than " + MaxNumberCharacterLength + " characters in length.");

            Span<char> oldBuffer = charBuffer;
            charBuffer = new char[Math.Min(charBuffer.Length * 2, MaxNumberCharacterLength)];
            oldBuffer.CopyTo(charBuffer.Slice(0, oldBuffer.Length));
        }

        /// <summary>
        /// Gets the requested lists. If there are previous list instances that can be reused, those are
        /// returned. Otherwise, new lists are created where necessary.
        /// </summary>
        private static void GetLists(bool loadNormals, bool loadColors, bool loadTexCoords, out List<Vector3> positions,
            out List<Vector3> normals, out List<Color4b> colors, out List<Vector2> texCoords, out List<int> indices)
        {
            lock (listsLock)
            {
                if (indicesReference != null && indicesReference.TryGetTarget(out indices))
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
            lock (listsLock)
            {
                if (indicesReference == null)
                    indicesReference = new WeakReference<List<int>>(null);

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

        /// <summary>
        /// Calculates the bounds that contain a list of vertices.
        /// </summary>
        /// <typeparam name="T">The type of vertex. The first 12 bytes of this struct should be the X, Y and Z floats.</typeparam>
        /// <param name="vertices">The vertices of which to calculate the bounds.</param>
        /// <param name="min">The minimum coordinates found in the vertices.</param>
        /// <param name="max">The maximum coordinates found in the vertices.</param>
        /// <returns>The size of the model, equal to max-min.</returns>
        /// <remarks>
        /// This function will not work properly if the (X, Y, Z) floats aren't in the first 12 bytes of the vertices.
        /// </remarks>
        public static Vector3 MeasureModel<T>(ReadOnlySpan<T> vertices, out Vector3 min, out Vector3 max) where T : unmanaged
        {
            if (Marshal.SizeOf<T>() < 12)
                throw new InvalidOperationException("Can't measure model: vertex size is less than 12 bytes.");

            min = default;
            max = default;

            // To make this work with any type of vertex, we use pointers to extract the position.
            T tmp;
            Vector3 tmpPosition;
            for (int i = 0; i < vertices.Length; i++)
            {
                tmp = vertices[i];
                unsafe
                {
                    tmpPosition = *((Vector3*)&tmp);
                }
                min = Vector3.Min(min, tmpPosition);
                max = Vector3.Max(max, tmpPosition);
            }

            return max - min;
        }
    }

    /// <summary>
    /// Specifies options for loading OBJ files.
    /// </summary>
    [Flags]
    public enum ObjLoadOptions
    {
        /// <summary>Specifies default options when loading an OBJ file.</summary>
        None = 0,

        /// <summary>Any face with more than 3 vertices is stripped down to just the first 3 vertices.</summary>
        TrianglesOnly = 1
    }
}
