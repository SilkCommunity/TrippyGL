namespace IndexedRendering
{
    /// <summary>
    /// A static class with the vertex and index data for this test
    /// </summary>
    static class Indices
    {
        /// <summary>
        /// All the vertices needed to make any number in a 7 segment display format,
        /// ordered as shown in the "indices.png" image
        /// </summary>
        public static SimpleVertex[] Vertices = new SimpleVertex[27]
        {
            new SimpleVertex(),
            new SimpleVertex(-0.3f, 0.9f),
            new SimpleVertex(0.3f, 0.9f),
            new SimpleVertex(-0.4f, 0.8f),
            new SimpleVertex(0.4f, 0.8f), //4
            new SimpleVertex(-0.5f, 0.7f),
            new SimpleVertex(-0.3f, 0.7f),
            new SimpleVertex(0.3f, 0.7f),
            new SimpleVertex(0.5f, 0.7f), //8
            new SimpleVertex(-0.5f, 0.1f),
            new SimpleVertex(-0.3f, 0.1f),
            new SimpleVertex(0.3f, 0.1f),
            new SimpleVertex(0.5f, 0.1f), //12
            new SimpleVertex(-0.4f, 0),
            new SimpleVertex(0.4f, 0),
            new SimpleVertex(-0.5f, -0.1f),
            new SimpleVertex(-0.3f, -0.1f), //16
            new SimpleVertex(0.3f, -0.1f),
            new SimpleVertex(0.5f, -0.1f),
            new SimpleVertex(-0.5f, -0.7f),
            new SimpleVertex(-0.3f, -0.7f), //20
            new SimpleVertex(0.3f, -0.7f),
            new SimpleVertex(0.5f, -0.7f),
            new SimpleVertex(-0.4f, -0.8f),
            new SimpleVertex(0.4f, -0.8f), //24
            new SimpleVertex(-0.3f, -0.9f),
            new SimpleVertex(0.3f, -0.9f),
        };

        // The following arrays store the index data for each number.
        // The indices point to the triangles that make up each number.

        // This could be optimized, since some numbers overlap it is possible to,
        // for example, make the 6 start at the same location than the 8 but
        // have the 6 not use the last segment. This would result in much less index data,
        // but for the sake of simplicity we'll do it like this here.

        public static byte[] Number0 = new byte[]
        {
            23, 20, 25, 20, 21, 25, 21, 25, 26, 26, 21, 24,
            19, 20, 23, 19, 20, 16, 19, 16, 15, 15, 16, 13,
            21, 22, 24, 21, 22, 18, 21, 18, 17, 17, 18, 14,
            9, 10, 13, 9, 10, 6, 9, 6, 5, 5, 6, 3,
            3, 1, 6, 1, 6, 7, 1, 7, 2, 2, 7, 4,
            11, 12, 14, 11, 12, 8, 11, 8, 7, 7, 8, 4
        };

        public static byte[] Number1 = new byte[]
        {
            21, 22, 24, 21, 22, 18, 21, 18, 17, 17, 18, 14,
            14, 12, 11, 11, 12, 8, 11, 8, 7, 7, 8, 4
        };

        public static byte[] Number2 = new byte[]
        {
            21, 24, 26, 21, 26, 25, 21, 25, 20, 20, 25, 23,
            23, 20, 19, 19, 20, 16, 19, 16, 15, 15, 13, 16,
            13, 10, 16, 10, 16, 17, 10, 17, 11, 11, 14, 17,
            11, 12, 14, 11, 12, 8, 11, 8, 7, 7, 8, 4,
            7, 4, 2, 2, 7, 6, 6, 2, 1, 1, 6, 3
        };

        public static byte[] Number3 = new byte[]
        {
            23, 20, 25, 25, 20, 26, 20, 26, 21, 21, 26, 24,
            21, 22, 24, 21, 22, 18, 18, 21, 17, 17, 18, 14,
            14, 11, 17, 17, 11, 10, 10, 17, 16, 10, 16, 13,
            11, 12, 14, 11, 12, 8, 8, 11, 7, 7, 8, 4,
            4, 2, 7, 7, 2, 6, 6, 2, 1, 1, 6, 3
        };

        public static byte[] Number4 = new byte[]
        {
            24, 21, 22, 22, 21, 18, 21, 18, 17, 17, 18, 14,
            14, 11, 12, 11, 12, 8, 8, 11, 7, 7, 8, 4,
            14, 11, 17, 17, 11, 16, 16, 11, 10, 10, 16, 13,
            9, 10, 13, 9, 10, 6, 6, 9, 5, 5, 6, 3
        };

        public static byte[] Number5 = new byte[]
        {
            23, 20, 25, 20, 25, 26, 20, 26, 21, 21, 26, 24,
            24, 21, 22, 21, 22, 18, 18, 21, 17, 17, 18, 14,
            14, 11, 17, 11, 17, 16, 16, 11, 10, 10, 16, 13,
            13, 9, 10, 9, 10, 6, 6, 9, 5, 5, 6, 3,
            3, 1, 6, 6, 2, 1, 2, 6, 7, 2, 7, 4
        };

        public static byte[] Number6 = new byte[]
        {
            23, 20, 25, 20, 21, 25, 21, 25, 26, 26, 21, 24,
            19, 20, 23, 19, 20, 16, 19, 16, 15, 15, 16, 13,
            21, 22, 24, 21, 22, 18, 21, 18, 17, 17, 18, 14,
            13, 10, 16, 10, 16, 17, 10, 17, 11, 11, 17, 14,
            9, 10, 13, 9, 10, 6, 9, 6, 5, 5, 6, 3,
            3, 1, 6, 1, 6, 7, 1, 7, 2, 2, 7, 4
        };

        public static byte[] Number7 = new byte[]
        {
            21, 22, 24, 21, 22, 18, 21, 18, 17, 17, 18, 14,
            14, 11, 12, 11, 12, 8, 8, 11, 7, 7, 8, 4,
            4, 2, 7, 2, 7, 1, 1, 7, 6, 6, 1, 3
        };

        public static byte[] Number8 = new byte[]
        {
            23, 20, 25, 20, 21, 25, 21, 25, 26, 26, 21, 24,
            19, 20, 23, 19, 20, 16, 19, 16, 15, 15, 16, 13,
            21, 22, 24, 21, 22, 18, 21, 18, 17, 17, 18, 14,
            13, 10, 16, 10, 16, 17, 10, 17, 11, 11, 17, 14,
            9, 10, 13, 9, 10, 6, 9, 6, 5, 5, 6, 3,
            3, 1, 6, 1, 6, 7, 1, 7, 2, 2, 7, 4,
            11, 12, 14, 11, 12, 8, 11, 8, 7, 7, 8, 4
        };

        public static byte[] Number9 = new byte[]
        {
            21, 22, 24, 21, 22, 18, 21, 18, 17, 17, 18, 14,
            14, 11, 12, 11, 12, 8, 8, 11, 7, 7, 8, 4,
            4, 2, 7, 2, 7, 1, 1, 7, 6, 6, 1, 3,
            3, 6, 5, 5, 6, 10, 5, 10, 9, 9, 10, 13,
            13, 10, 16, 10, 16, 17, 10, 17, 11, 11, 17, 14
        };

        public static byte[][] AllNumbersIndices = new byte[][]
        {
            Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9
        };

        public static int TotalIndicesLength = Number0.Length + Number1.Length
            + Number2.Length + Number3.Length + Number4.Length + Number5.Length
            + Number6.Length + Number7.Length + Number8.Length + Number9.Length;
    }
}
