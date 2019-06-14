using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Contains various methods used throughout the library
    /// </summary>
    public static class TrippyUtils
    {
        /// <summary>
        /// Returns whether the specified data base type is of integer format (such as byte, ushort, int, uint)
        /// </summary>
        public static bool IsVertexAttribIntegerType(VertexAttribPointerType dataBaseType)
        {
            return dataBaseType == VertexAttribPointerType.UnsignedByte || dataBaseType == VertexAttribPointerType.Byte
                || dataBaseType == VertexAttribPointerType.UnsignedShort || dataBaseType == VertexAttribPointerType.Short
                || dataBaseType == VertexAttribPointerType.UnsignedInt || dataBaseType == VertexAttribPointerType.Int
                || dataBaseType == VertexAttribPointerType.Int2101010Rev || dataBaseType == VertexAttribPointerType.UnsignedInt2101010Rev
                || dataBaseType == VertexAttribPointerType.UnsignedInt10F11F11FRev;
        }

        /// <summary>
        /// Gets whether the specified attrib type is an integer type (such as int, ivecX, uint or uvecX)
        /// </summary>
        public static bool IsVertexAttribIntegerType(ActiveAttribType attribType)
        {
            return attribType == ActiveAttribType.Int || attribType == ActiveAttribType.UnsignedInt
                || (attribType >= ActiveAttribType.IntVec2 && attribType <= ActiveAttribType.IntVec4)
                || (attribType >= ActiveAttribType.UnsignedIntVec2 && attribType <= ActiveAttribType.UnsignedIntVec4);
        }

        /// <summary>
        /// Gets whether the specified attrib type is a floating point double type (such as double, dvecX or dmatMxN)
        /// </summary>
        public static bool IsVertexAttribDoubleType(ActiveAttribType attribType)
        {
            return attribType == ActiveAttribType.Double
                || (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4)
                || (attribType >= ActiveAttribType.DoubleMat2 && attribType <= ActiveAttribType.DoubleMat4x3);
        }

        /// <summary>
        /// Gets the corresponding variables for the specified ActiveAttribType
        /// </summary>
        /// <param name="attribType">The attribute type to query</param>
        /// <param name="indexUseCount">The amount of attribute indices it will need</param>
        /// <param name="size">The amount of components each index will have</param>
        /// <param name="type">The base type of each component</param>
        public static void GetVertexAttribTypeData(ActiveAttribType attribType, out int indexUseCount, out int size, out VertexAttribPointerType type)
        {
            indexUseCount = GetVertexAttribTypeIndexCount(attribType);
            size = GetVertexAttribTypeSize(attribType);
            type = GetVertexAttribBaseType(attribType);
        }

        /// <summary>
        /// Gets the base variable type for the specified attribute type
        /// (for example, vec4 would return float. dmat2 would return double, ivec2 returns int)
        /// </summary>
        public static VertexAttribPointerType GetVertexAttribBaseType(ActiveAttribType attribType)
        {
            if (attribType == ActiveAttribType.Float // is it a float?
                || (attribType >= ActiveAttribType.FloatVec2 && attribType <= ActiveAttribType.FloatVec4) // or is it a float vector?
                || (attribType >= ActiveAttribType.FloatMat2 && attribType <= ActiveAttribType.FloatMat4x3)) // or is it a float matrix?
                return VertexAttribPointerType.Float;

            if (attribType == ActiveAttribType.Int
                || (attribType >= ActiveAttribType.IntVec2 && attribType <= ActiveAttribType.IntVec4))
                return VertexAttribPointerType.Int;

            if (attribType == ActiveAttribType.Double
                || (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4)
                || (attribType >= ActiveAttribType.DoubleMat2 && attribType <= ActiveAttribType.DoubleMat4x3))
                return VertexAttribPointerType.Double;

            if (attribType == ActiveAttribType.UnsignedInt
                || (attribType >= ActiveAttribType.UnsignedIntVec2 || attribType <= ActiveAttribType.UnsignedIntVec4))
                return VertexAttribPointerType.UnsignedInt;

            throw new ArgumentException("The provided value is not a valid enum value", "attribType");
        }

        /// <summary>
        /// Gets the attribute's size. By size, this means "vector size" (float is 1, vec2i is 2, bvec4 is 4, etc)
        /// </summary>
        public static int GetVertexAttribTypeSize(ActiveAttribType attribType)
        {
            if ((attribType >= ActiveAttribType.Int && attribType <= ActiveAttribType.Float) || attribType == ActiveAttribType.Double)
                return 1;

            if (attribType >= ActiveAttribType.FloatVec2 && attribType <= ActiveAttribType.FloatVec4)
                return attribType - ActiveAttribType.FloatVec2 + 2;

            if (attribType >= ActiveAttribType.IntVec2 && attribType <= ActiveAttribType.IntVec4)
                return attribType - ActiveAttribType.IntVec2 + 2;

            if (attribType >= ActiveAttribType.UnsignedIntVec2 && attribType <= ActiveAttribType.UnsignedIntVec4)
                return attribType - ActiveAttribType.UnsignedIntVec2 + 2;

            if (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4)
                return attribType - ActiveAttribType.DoubleVec2 + 2;

            switch (attribType)
            {
                case ActiveAttribType.FloatMat2:
                case ActiveAttribType.FloatMat3x2:
                case ActiveAttribType.FloatMat4x2:
                case ActiveAttribType.DoubleMat2:
                case ActiveAttribType.DoubleMat3x2:
                case ActiveAttribType.DoubleMat4x2:
                    return 2;

                case ActiveAttribType.FloatMat3:
                case ActiveAttribType.FloatMat2x3:
                case ActiveAttribType.FloatMat4x3:
                case ActiveAttribType.DoubleMat3:
                case ActiveAttribType.DoubleMat2x3:
                case ActiveAttribType.DoubleMat4x3:
                    return 3;

                case ActiveAttribType.FloatMat4:
                case ActiveAttribType.FloatMat2x4:
                case ActiveAttribType.FloatMat3x4:
                case ActiveAttribType.DoubleMat4:
                case ActiveAttribType.DoubleMat2x4:
                case ActiveAttribType.DoubleMat3x4:
                    return 4;
            }

            throw new ArgumentException("The provided value is not a valid enum value", "attribType");
        }

        /// <summary>
        /// Gets the amount of indices the vertex attribute occupies
        /// </summary>
        public static int GetVertexAttribTypeIndexCount(ActiveAttribType attribType)
        {
            if ((attribType >= ActiveAttribType.Int && attribType <= ActiveAttribType.Float)
                || attribType == ActiveAttribType.Double
                || (attribType >= ActiveAttribType.FloatVec2 && attribType <= ActiveAttribType.IntVec4)
                || (attribType >= ActiveAttribType.UnsignedIntVec2 && attribType <= ActiveAttribType.UnsignedIntVec4)
                || (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4))
                return 1;

            switch (attribType)
            {
                case ActiveAttribType.FloatMat2:
                case ActiveAttribType.FloatMat2x3:
                case ActiveAttribType.FloatMat2x4:
                case ActiveAttribType.DoubleMat2:
                case ActiveAttribType.DoubleMat2x3:
                case ActiveAttribType.DoubleMat2x4:
                    return 2;

                case ActiveAttribType.FloatMat3:
                case ActiveAttribType.FloatMat3x2:
                case ActiveAttribType.FloatMat3x4:
                case ActiveAttribType.DoubleMat3:
                case ActiveAttribType.DoubleMat3x2:
                case ActiveAttribType.DoubleMat3x4:
                    return 3;

                case ActiveAttribType.FloatMat4:
                case ActiveAttribType.FloatMat4x2:
                case ActiveAttribType.FloatMat4x3:
                case ActiveAttribType.DoubleMat4:
                case ActiveAttribType.DoubleMat4x2:
                case ActiveAttribType.DoubleMat4x3:
                    return 4;
            }

            throw new ArgumentException("The provided value is not a valid enum value", "attribType");
        }

        /// <summary>
        /// Gets the size in bytes of an attribute type
        /// </summary>
        public static int GetVertexAttribSizeInBytes(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Byte:
                case VertexAttribPointerType.UnsignedByte:
                    return 1;

                case VertexAttribPointerType.Short:
                case VertexAttribPointerType.UnsignedShort:
                case VertexAttribPointerType.HalfFloat:
                    return 2;

                case VertexAttribPointerType.Float:
                case VertexAttribPointerType.Int:
                case VertexAttribPointerType.UnsignedInt:
                case VertexAttribPointerType.Fixed:
                    return 4;

                case VertexAttribPointerType.Double:
                    return 8;

                //case VertexAttribPointerType.Int2101010Rev:
                //case VertexAttribPointerType.UnsignedInt10F11F11FRev:
                //case VertexAttribPointerType.UnsignedInt2101010Rev:
                default:
                    throw new NotSupportedException("The specified vertex attribute format's size in bytes cannot be deciphered by the pointer type.");

            }
        }

        /// <summary>
        /// Returns whether a DepthStencilFormat is a depth-only format
        /// </summary>
        /// <param name="format">The DepthStencilFormat value to check</param>
        public static bool IsDepthStencilFormatDepthOnly(DepthStencilFormat format)
        {
            return (format >= DepthStencilFormat.Depth16 && format <= DepthStencilFormat.Depth32) || format == DepthStencilFormat.Depth32f;
        }

        /// <summary>
        /// Returns whether a DepthStencilFormat is a depth-and-stencil format
        /// </summary>
        /// <param name="format">The DepthStencilFormat value to check</param>
        public static bool IsDepthStencilFormatDepthAndStencil(DepthStencilFormat format)
        {
            return format == DepthStencilFormat.Depth24Stencil8 || format == DepthStencilFormat.Depth32fStencil8;
        }

        /// <summary>
        /// Returns whether a DepthStencilFormat is a stencil-only format
        /// </summary>
        /// <param name="format">The DepthStencilFormat value to check</param>
        public static bool IsDepthStencilFormatStencilOnly(DepthStencilFormat format)
        {
            return format == DepthStencilFormat.Stencil8;
        }


        /// <summary>
        /// Returns whether the given ActiveUniformType is a sampler type
        /// </summary>
        /// <param name="type">The type of uniform to compare</param>
        public static bool IsUniformSamplerType(ActiveUniformType type)
        {
            return (type >= ActiveUniformType.Sampler1D && type <= ActiveUniformType.Sampler2DRectShadow)
                || (type >= ActiveUniformType.Sampler1DArray && type <= ActiveUniformType.SamplerCubeShadow)
                || (type >= ActiveUniformType.IntSampler1D && type <= ActiveUniformType.UnsignedIntSamplerBuffer)
                || (type >= ActiveUniformType.SamplerCubeMapArray && type <= ActiveUniformType.UnsignedIntSamplerCubeMapArray)
                || (type >= ActiveUniformType.Sampler2DMultisample && type <= ActiveUniformType.UnsignedIntSampler2DMultisampleArray);
        }

        /// <summary>
        /// Turns a value from the TextureImageFormat enum into the necessary enums to create an OpenGL texture's image storage
        /// </summary>
        /// <param name="imageFormat">The requested image format</param>
        /// <param name="pixelInternalFormat">The pixel's internal format</param>
        /// <param name="pixelType">The pixel's type</param>
        public static void GetTextureFormatEnums(TextureImageFormat imageFormat, out PixelInternalFormat pixelInternalFormat, out PixelType pixelType)
        {
            // The workings of this function are related to the numbers assigned to each enum value
            int b = (int)imageFormat / 32;

            switch (b)
            {
                case 0: 
                    #region UnsignedByteTypes
                    pixelType = PixelType.UnsignedByte;
                    pixelInternalFormat = PixelInternalFormat.Rgba8;
                    //this works because the only unsigned byte type is Color4b
                    return;
                #endregion
                case 1:
                    #region FloatTypes
                    pixelType = PixelType.Float;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelInternalFormat = PixelInternalFormat.R32f;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32f;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32f;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32f;
                            return;
                    }
                    break;
                #endregion
                case 2:
                    #region IntTypes
                    pixelType = PixelType.Int;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelInternalFormat = PixelInternalFormat.R32i;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32i;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32i;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32i;
                            return;
                    }
                    break;
                #endregion
                case 3:
                    #region UnsignedIntTypes
                    pixelType = PixelType.UnsignedInt;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelInternalFormat = PixelInternalFormat.R32ui;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32ui;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32ui;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32ui;
                            return;
                    }
                    break;
                    #endregion
            }
            throw new ArgumentException("Image format is not a valid TextureImageFormat value", "imageFormat");
        }

        /// <summary>
        /// Gets whether the specified texture type is mipmappable
        /// </summary>
        /// <param name="textureType">The texture type to check</param>
        public static bool IsTextureTypeMipmappable(TextureTarget textureType)
        {
            return textureType == TextureTarget.Texture1D || textureType == TextureTarget.Texture2D || textureType == TextureTarget.Texture3D
                || textureType == TextureTarget.Texture1DArray || textureType == TextureTarget.Texture2DArray
                || textureType == TextureTarget.TextureCubeMap || textureType == TextureTarget.TextureCubeMapArray;
        }

        /// <summary>
        /// Gets the size in bytes for one element of the specified type.
        /// If the provided type isn't GL_UNSIGNED_BYTE, GL_UNSIGNED_SHORT or GL_UNSIGNED_INT, this method throws an exception
        /// </summary>
        /// <param name="type">The type of element</param>
        public static int GetSizeInBytesOfType(DrawElementsType type)
        {
            switch (type)
            {
                case DrawElementsType.UnsignedByte:
                    return 1;
                case DrawElementsType.UnsignedShort:
                    return 2;
                case DrawElementsType.UnsignedInt:
                    return 4;
            }

            throw new ArgumentException("That's not a valid DrawElementsType value");
        }

        /// <summary>
        /// Returns whether the given TextureImageFormat represents a format with integer base type
        /// </summary>
        /// <param name="imageFormat">The image format to check</param>
        public static bool IsImageFormatIntegerType(TextureImageFormat imageFormat)
        {
            return (imageFormat >= TextureImageFormat.Int && imageFormat <= TextureImageFormat.Vector4i)
                || (imageFormat >= TextureImageFormat.UnsignedInt && imageFormat <= TextureImageFormat.UVector4i);

        }
    }
}
