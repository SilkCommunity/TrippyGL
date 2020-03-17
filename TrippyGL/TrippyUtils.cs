using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Contains various methods used throughout the library.
    /// </summary>
    public static class TrippyUtils
    {
        /// <summary>
        /// Returns whether the specified data base type is of integer format (such as byte, ushort, int, uint).
        /// </summary>
        public static bool IsVertexAttribIntegerType(VertexAttribPointerType dataBaseType)
        {
            return dataBaseType >= VertexAttribPointerType.Byte && dataBaseType <= VertexAttribPointerType.UnsignedInt;
            // accepted: Byte, UnsignedByte, Short, UnsignedShort, Int, UnsignedInt
        }

        /// <summary>
        /// Gets whether the specified attrib type is an integer type (such as int, ivecX, uint or uvecX).
        /// </summary>
        public static bool IsVertexAttribIntegerType(AttributeType attribType)
        {
            return attribType == AttributeType.Int || attribType == AttributeType.UnsignedInt
                || (attribType >= AttributeType.IntVec2 && attribType <= AttributeType.IntVec4)
                || (attribType >= AttributeType.UnsignedIntVec2 && attribType <= AttributeType.UnsignedIntVec4);
        }

        /// <summary>
        /// Gets whether the specified attrib type is a floating point single type (such as float, vecX, matMxN).
        /// </summary>
        public static bool IsVertexAttribFloatType(AttributeType attribType)
        {
            return attribType == AttributeType.Float
                || (attribType >= AttributeType.FloatVec2 && attribType <= AttributeType.FloatVec4)
                || (attribType >= AttributeType.FloatMat2 && attribType <= AttributeType.FloatMat4x3);
        }

        /// <summary>
        /// Gets whether the specified attrib type is a floating point double type (such as double, dvecX or dmatMxN).
        /// </summary>
        public static bool IsVertexAttribDoubleType(AttributeType attribType)
        {
            return attribType == AttributeType.Double
                || (attribType >= AttributeType.DoubleVec2 && attribType <= AttributeType.DoubleVec4)
                || (attribType >= AttributeType.DoubleMat2 && attribType <= AttributeType.DoubleMat4x3);
        }

        /// <summary>
        /// Gets the corresponding variables for the specified ActiveAttribType.
        /// </summary>
        /// <param name="attribType">The attribute type to query.</param>
        /// <param name="indexUseCount">The amount of attribute indices it will need.</param>
        /// <param name="size">The amount of components each index will have.</param>
        /// <param name="type">The base type of each component.</param>
        public static void GetVertexAttribTypeData(AttributeType attribType, out uint indexUseCount, out int size, out VertexAttribPointerType type)
        {
            indexUseCount = GetVertexAttribTypeIndexCount(attribType);
            size = GetVertexAttribTypeSize(attribType);
            type = GetVertexAttribBaseType(attribType);
        }

        /// <summary>
        /// Gets the base variable type for the specified attribute type
        /// (for example, vec4 would return float. dmat2 would return double, ivec2 returns int).
        /// </summary>
        public static VertexAttribPointerType GetVertexAttribBaseType(AttributeType attribType)
        {
            if (attribType == AttributeType.Float // is it a float?
                || (attribType >= AttributeType.FloatVec2 && attribType <= AttributeType.FloatVec4) // or is it a float vector?
                || (attribType >= AttributeType.FloatMat2 && attribType <= AttributeType.FloatMat4x3)) // or is it a float matrix?
                return VertexAttribPointerType.Float;

            if (attribType == AttributeType.Int
                || (attribType >= AttributeType.IntVec2 && attribType <= AttributeType.IntVec4))
                return VertexAttribPointerType.Int;

            if (attribType == AttributeType.Double
                || (attribType >= AttributeType.DoubleVec2 && attribType <= AttributeType.DoubleVec4)
                || (attribType >= AttributeType.DoubleMat2 && attribType <= AttributeType.DoubleMat4x3))
                return VertexAttribPointerType.Double;

            if (attribType == AttributeType.UnsignedInt
                || attribType >= AttributeType.UnsignedIntVec2 || attribType <= AttributeType.UnsignedIntVec4)
                return VertexAttribPointerType.UnsignedInt;

            throw new ArgumentException("The provided value is not a valid enum value", nameof(attribType));
        }

        /// <summary>
        /// Gets the attribute's size. By size, this means "vector size" (float is 1, vec2i is 2, bvec4 is 4, etc).
        /// </summary>
        public static int GetVertexAttribTypeSize(AttributeType attribType)
        {
            if ((attribType >= AttributeType.Int && attribType <= AttributeType.Float) || attribType == AttributeType.Double)
                return 1;

            if (attribType >= AttributeType.FloatVec2 && attribType <= AttributeType.FloatVec4)
                return attribType - AttributeType.FloatVec2 + 2;

            if (attribType >= AttributeType.IntVec2 && attribType <= AttributeType.IntVec4)
                return attribType - AttributeType.IntVec2 + 2;

            if (attribType >= AttributeType.UnsignedIntVec2 && attribType <= AttributeType.UnsignedIntVec4)
                return attribType - AttributeType.UnsignedIntVec2 + 2;

            if (attribType >= AttributeType.DoubleVec2 && attribType <= AttributeType.DoubleVec4)
                return attribType - AttributeType.DoubleVec2 + 2;

            switch (attribType)
            {
                case AttributeType.FloatMat2:
                case AttributeType.FloatMat3x2:
                case AttributeType.FloatMat4x2:
                case AttributeType.DoubleMat2:
                case AttributeType.DoubleMat3x2:
                case AttributeType.DoubleMat4x2:
                    return 2;

                case AttributeType.FloatMat3:
                case AttributeType.FloatMat2x3:
                case AttributeType.FloatMat4x3:
                case AttributeType.DoubleMat3:
                case AttributeType.DoubleMat2x3:
                case AttributeType.DoubleMat4x3:
                    return 3;

                case AttributeType.FloatMat4:
                case AttributeType.FloatMat2x4:
                case AttributeType.FloatMat3x4:
                case AttributeType.DoubleMat4:
                case AttributeType.DoubleMat2x4:
                case AttributeType.DoubleMat3x4:
                    return 4;
            }

            throw new ArgumentException("The provided value is not a valid enum value", nameof(attribType));
        }

        /// <summary>
        /// Gets the amount of indices the vertex attribute occupies.
        /// </summary>
        public static uint GetVertexAttribTypeIndexCount(AttributeType attribType)
        {
            if ((attribType >= AttributeType.Int && attribType <= AttributeType.Float)
                || attribType == AttributeType.Double
                || (attribType >= AttributeType.FloatVec2 && attribType <= AttributeType.IntVec4)
                || (attribType >= AttributeType.UnsignedIntVec2 && attribType <= AttributeType.UnsignedIntVec4)
                || (attribType >= AttributeType.DoubleVec2 && attribType <= AttributeType.DoubleVec4))
                return 1;

            switch (attribType)
            {
                case AttributeType.FloatMat2:
                case AttributeType.FloatMat2x3:
                case AttributeType.FloatMat2x4:
                case AttributeType.DoubleMat2:
                case AttributeType.DoubleMat2x3:
                case AttributeType.DoubleMat2x4:
                    return 2;

                case AttributeType.FloatMat3:
                case AttributeType.FloatMat3x2:
                case AttributeType.FloatMat3x4:
                case AttributeType.DoubleMat3:
                case AttributeType.DoubleMat3x2:
                case AttributeType.DoubleMat3x4:
                    return 3;

                case AttributeType.FloatMat4:
                case AttributeType.FloatMat4x2:
                case AttributeType.FloatMat4x3:
                case AttributeType.DoubleMat4:
                case AttributeType.DoubleMat4x2:
                case AttributeType.DoubleMat4x3:
                    return 4;
            }

            throw new ArgumentException("The provided value is not a valid enum value", nameof(attribType));
        }

        /// <summary>
        /// Gets the size in bytes of an attribute type.
        /// </summary>
        public static uint GetVertexAttribSizeInBytes(VertexAttribPointerType type)
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

                default:
                    throw new NotSupportedException("The specified vertex attribute format's size in bytes cannot be deciphered by the pointer type.");

            }
        }

        /// <summary>
        /// Copies the given VertexAttribDescription array into a new array, but doesn't copy the descriptions that are only used for padding.
        /// </summary>
        public static VertexAttribDescription[] CopyVertexAttribDescriptionsWithoutPaddingDescriptors(ReadOnlySpan<VertexAttribDescription> descriptions)
        {
            // TODO: Make this output into a Span somehow

            int length = 0; // First we calculate the length of the new array
            for (int i = 0; i < descriptions.Length; i++)
                if (!descriptions[i].IsPadding)
                    length++;

            VertexAttribDescription[] res = new VertexAttribDescription[length];
            int index = 0; // Then we copy the non-padding elements to the new array
            for (int i = 0; i < descriptions.Length; i++)
                if (!descriptions[i].IsPadding)
                    res[index++] = descriptions[i];

            return res; // Finally we return the new array
        }



        /// <summary>
        /// Returns whether a DepthStencilFormat is a depth-only format.
        /// </summary>
        /// <param name="format">The DepthStencilFormat value to check.</param>
        public static bool IsDepthStencilFormatDepthOnly(DepthStencilFormat format)
        {
            return format == DepthStencilFormat.Depth16 || format == DepthStencilFormat.Depth24 || format == DepthStencilFormat.Depth32f;
        }

        /// <summary>
        /// Returns whether a DepthStencilFormat is a depth-and-stencil format.
        /// </summary>
        /// <param name="format">The DepthStencilFormat value to check.</param>
        public static bool IsDepthStencilFormatDepthAndStencil(DepthStencilFormat format)
        {
            return format == DepthStencilFormat.Depth24Stencil8 || format == DepthStencilFormat.Depth32fStencil8;
        }

        /// <summary>
        /// Returns whether a DepthStencilFormat is a stencil-only format.
        /// </summary>
        /// <param name="format">The DepthStencilFormat value to check.</param>
        public static bool IsDepthStencilFormatStencilOnly(DepthStencilFormat format)
        {
            return format == DepthStencilFormat.Stencil8;
        }



        /// <summary>
        /// Returns whether the given ActiveUniformType is a sampler type. This includes sampler-array types.
        /// </summary>
        /// <param name="type">The type of uniform to compare.</param>
        public static bool IsUniformSamplerType(UniformType type)
        {
            return (type >= UniformType.Sampler1D && type <= UniformType.Sampler2DRectShadow)
                || (type >= UniformType.Sampler1DArray && type <= UniformType.SamplerCubeShadow)
                || (type >= UniformType.IntSampler1D && type <= UniformType.UnsignedIntSamplerBuffer)
                || (type >= UniformType.SamplerCubeMapArray && type <= UniformType.UnsignedIntSamplerCubeMapArray)
                || (type >= UniformType.Sampler2DMultisample && type <= UniformType.UnsignedIntSampler2DMultisampleArray);
        }

        /// <summary>
        /// Turns a value from the TextureImageFormat enum into the necessary enums to create an OpenGL texture's image storage.
        /// </summary>
        /// <param name="imageFormat">The requested image format.</param>
        /// <param name="pixelInternalFormat">The pixel's internal format.</param>
        /// <param name="pixelType">The pixel's type.</param>
        public static void GetTextureFormatEnums(TextureImageFormat imageFormat, out InternalFormat pixelInternalFormat, out PixelType pixelType, out PixelFormat pixelFormat)
        {
            // The workings of this function are related to the numbers assigned to each enum value
            int b = (int)imageFormat / 32;

            switch (b)
            {
                case 0:
                    #region UnsignedByteTypes
                    pixelType = PixelType.UnsignedByte;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 5:
                            pixelInternalFormat = InternalFormat.Rgba8;
                            pixelFormat = PixelFormat.Rgba;
                            return;
                    }
                    break;
                #endregion
                case 1:
                    #region FloatTypes
                    pixelType = PixelType.Float;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelInternalFormat = InternalFormat.R32f;
                            pixelFormat = PixelFormat.Red;
                            return;
                        case 2:
                            pixelInternalFormat = InternalFormat.RG32f;
                            pixelFormat = PixelFormat.RG;
                            return;
                        case 3:
                            pixelInternalFormat = InternalFormat.Rgb32f;
                            pixelFormat = PixelFormat.Rgb;
                            return;
                        case 4:
                            pixelInternalFormat = InternalFormat.Rgba32f;
                            pixelFormat = PixelFormat.Rgba;
                            return;
                        case 5:
                            pixelInternalFormat = InternalFormat.DepthComponent16;
                            pixelFormat = PixelFormat.DepthComponent;
                            return;
                        case 6:
                            pixelInternalFormat = InternalFormat.DepthComponent24Arb;
                            pixelFormat = PixelFormat.DepthComponent;
                            return;
                        case 7:
                            pixelInternalFormat = InternalFormat.DepthComponent32f;
                            pixelFormat = PixelFormat.DepthComponent;
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
                            pixelInternalFormat = InternalFormat.R32i;
                            pixelFormat = PixelFormat.RgbaInteger;
                            return;
                        case 2:
                            pixelInternalFormat = InternalFormat.RG32i;
                            pixelFormat = PixelFormat.RgbaInteger;
                            return;
                        case 3:
                            pixelInternalFormat = InternalFormat.Rgb32i;
                            pixelFormat = PixelFormat.RgbaInteger;
                            return;
                        case 4:
                            pixelInternalFormat = InternalFormat.Rgba32i;
                            pixelFormat = PixelFormat.RgbaInteger;
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
                            pixelInternalFormat = InternalFormat.R32ui;
                            pixelFormat = PixelFormat.RedInteger;
                            return;
                        case 2:
                            pixelInternalFormat = InternalFormat.RG32ui;
                            pixelFormat = PixelFormat.RGInteger;
                            return;
                        case 3:
                            pixelInternalFormat = InternalFormat.Rgb32ui;
                            pixelFormat = PixelFormat.RgbInteger;
                            return;
                        case 4:
                            pixelInternalFormat = InternalFormat.Rgba32ui;
                            pixelFormat = PixelFormat.RgbaInteger;
                            return;
                    }
                    break;
                #endregion
                case 4:
                    #region Depth24Stencil8
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelType = (PixelType)GLEnum.UnsignedInt248;
                            pixelInternalFormat = InternalFormat.Depth24Stencil8;
                            pixelFormat = PixelFormat.DepthStencil;
                            return;
                    }
                    break;
                    #endregion
            }

            throw new ArgumentException("Image format is not a valid TextureImageFormat value", nameof(imageFormat));
        }

        /// <summary>
        /// Gets the default pixel format for get data texture operations for the specified TextureImageFormat.
        /// </summary>
        /// <param name="format">The format to check.</param>
        public static PixelFormat GetDefaultGetDataFormat(TextureImageFormat format)
        {
            if (IsImageFormatColorRenderable(format))
                return PixelFormat.Rgba;
            else if (IsImageFormatDepthType(format))
                return PixelFormat.DepthComponent;
            else if (IsImageFormatDepthStencilType(format))
                return PixelFormat.DepthStencil;
            else if (IsImageFormatStencilType(format))
                return PixelFormat.StencilIndex;
            throw new ArgumentException("The given TextureImageFormat isn't valid");
        }

        /// <summary>
        /// Gets whether the specified texture type is mipmappable.
        /// </summary>
        /// <param name="textureType">The texture type to check.</param>
        public static bool IsTextureTypeMipmappable(TextureTarget textureType)
        {
            return textureType == TextureTarget.Texture1D || textureType == TextureTarget.Texture2D || textureType == TextureTarget.Texture3D
                || textureType == TextureTarget.Texture1DArray || textureType == TextureTarget.Texture2DArray
                || textureType == TextureTarget.TextureCubeMap || textureType == TextureTarget.TextureCubeMapArray;
        }

        /// <summary>
        /// Returns whether the given TextureImageFormat represents a format with integer base type.
        /// </summary>
        /// <param name="imageFormat">The image format to check.</param>
        public static bool IsImageFormatIntegerType(TextureImageFormat imageFormat)
        {
            return (imageFormat >= TextureImageFormat.Int && imageFormat <= TextureImageFormat.Int4)
                || (imageFormat >= TextureImageFormat.UnsignedInt && imageFormat <= TextureImageFormat.UnsignedInt4);
        }

        /// <summary>
        /// Returns whether the given TextureImageFromat represents a depth format.
        /// </summary>
        /// <param name="imageFormat">The image format to check.</param>
        public static bool IsImageFormatDepthType(TextureImageFormat imageFormat)
        {
            return imageFormat >= TextureImageFormat.Depth16 && imageFormat <= TextureImageFormat.Depth32f;
        }

        /// <summary>
        /// Returns whether the given TextureImageFromat represents a stencil format.
        /// </summary>
        /// <param name="imageFormat">The image format to check.</param>
        public static bool IsImageFormatStencilType(TextureImageFormat imageFormat)
        {
            return false; //there are no stencil-only image formats haha yes
        }

        /// <summary>
        /// Returns whether the given TextureImageFromat represents a depth-stencil format.
        /// </summary>
        /// <param name="imageFormat">The image format to check.</param>
        public static bool IsImageFormatDepthStencilType(TextureImageFormat imageFormat)
        {
            return imageFormat == TextureImageFormat.Depth24Stencil8;
        }

        /// <summary>
        /// Returns whether the given TextureImageFromat is color-renderable.
        /// </summary>
        /// <param name="imageFormat">The image format to check.</param>
        public static bool IsImageFormatColorRenderable(TextureImageFormat imageFormat)
        {
            return imageFormat == TextureImageFormat.Color4b
                || (imageFormat >= TextureImageFormat.Float && imageFormat <= TextureImageFormat.Float4)
                || (imageFormat >= TextureImageFormat.Int && imageFormat <= TextureImageFormat.Int4)
                || (imageFormat >= TextureImageFormat.UnsignedInt && imageFormat <= TextureImageFormat.UnsignedInt4);
        }



        /// <summary>
        /// Returns whether the specified FramebufferAttachmentPoint represents a color[i] attachment.
        /// </summary>
        /// <param name="attachment">The FramebufferAttachmentPoint value to check.</param>
        public static bool IsFramebufferAttachmentPointColor(FramebufferAttachmentPoint attachment)
        {
            int i = attachment - FramebufferAttachmentPoint.Color0;
            return i >= 0 && i < 32;
        }

        /// <summary>
        /// Returns whether the specified RenderbufferFormat represents a depth-only format.
        /// </summary>
        /// <param name="format">The format to check.</param>
        public static bool IsRenderbufferFormatDepthOnly(RenderbufferFormat format)
        {
            return format == RenderbufferFormat.Depth16 || format == RenderbufferFormat.Depth24 || format == RenderbufferFormat.Depth32f;
        }

        /// <summary>
        /// Returns whether the specified RenderbufferFormat represents a stencil-only format.
        /// </summary>
        /// <param name="format">The format to check.</param>
        public static bool IsRenderbufferFormatStencilOnly(RenderbufferFormat format)
        {
            return format == RenderbufferFormat.Stencil8;
        }

        /// <summary>
        /// Returns whether the specified RenderbufferFormat represents a depth-stencil format.
        /// </summary>
        /// <param name="format">The format to check.</param>
        public static bool IsRenderbufferFormatDepthStencil(RenderbufferFormat format)
        {
            return format == RenderbufferFormat.Depth24Stencil8 || format == RenderbufferFormat.Depth32fStencil8;
        }

        /// <summary>
        /// Returns whether the specified RenderbufferFormat represents a color renderable format.
        /// </summary>
        /// <param name="format">The format to check.</param>
        public static bool IsRenderbufferFormatColorRenderable(RenderbufferFormat format)
        {
            return format == RenderbufferFormat.Color4b || format == RenderbufferFormat.Float
                || format == RenderbufferFormat.Float2 || format == RenderbufferFormat.Float4
                || format == RenderbufferFormat.Int || format == RenderbufferFormat.Int2
                || format == RenderbufferFormat.Int4 || format == RenderbufferFormat.UnsignedInt
                || format == RenderbufferFormat.UnsignedInt2 || format == RenderbufferFormat.UnsignedInt4;
        }

        /// <summary>
        /// Gets the default valid framebuffer attachment point for a renderbuffer format. (depth/stencil/depthstencil/color0).
        /// </summary>
        /// <param name="format">The RenderbufferFormat to check for.</param>
        public static FramebufferAttachmentPoint GetCorrespondingRenderbufferFramebufferAttachmentPoint(RenderbufferFormat format)
        {
            if (IsRenderbufferFormatColorRenderable(format))
                return FramebufferAttachmentPoint.Color0;
            if (IsRenderbufferFormatDepthOnly(format))
                return FramebufferAttachmentPoint.Depth;
            if (IsRenderbufferFormatDepthStencil(format))
                return FramebufferAttachmentPoint.DepthStencil;
            if (IsRenderbufferFormatStencilOnly(format))
                return FramebufferAttachmentPoint.Stencil;
            throw new ArgumentException("The specified format appears to be invalid");
        }
    }
}
