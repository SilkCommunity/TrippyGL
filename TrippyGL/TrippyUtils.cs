using OpenTK.Graphics.OpenGL4;
using System;

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
        public static bool IsVertexAttribIntegerType(ActiveAttribType attribType)
        {
            return attribType == ActiveAttribType.Int || attribType == ActiveAttribType.UnsignedInt
                || (attribType >= ActiveAttribType.IntVec2 && attribType <= ActiveAttribType.IntVec4)
                || (attribType >= ActiveAttribType.UnsignedIntVec2 && attribType <= ActiveAttribType.UnsignedIntVec4);
        }

        /// <summary>
        /// Gets whether the specified attrib type is a floating point single type (such as float, vecX, matMxN).
        /// </summary>
        public static bool IsVertexAttribFloatType(ActiveAttribType attribType)
        {
            return attribType == ActiveAttribType.Float
                || (attribType >= ActiveAttribType.FloatVec2 && attribType <= ActiveAttribType.FloatVec4)
                || (attribType >= ActiveAttribType.FloatMat2 && attribType <= ActiveAttribType.FloatMat4x3);
        }

        /// <summary>
        /// Gets whether the specified attrib type is a floating point double type (such as double, dvecX or dmatMxN).
        /// </summary>
        public static bool IsVertexAttribDoubleType(ActiveAttribType attribType)
        {
            return attribType == ActiveAttribType.Double
                || (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4)
                || (attribType >= ActiveAttribType.DoubleMat2 && attribType <= ActiveAttribType.DoubleMat4x3);
        }

        /// <summary>
        /// Gets the corresponding variables for the specified ActiveAttribType.
        /// </summary>
        /// <param name="attribType">The attribute type to query.</param>
        /// <param name="indexUseCount">The amount of attribute indices it will need.</param>
        /// <param name="size">The amount of components each index will have.</param>
        /// <param name="type">The base type of each component.</param>
        public static void GetVertexAttribTypeData(ActiveAttribType attribType, out int indexUseCount, out int size, out VertexAttribPointerType type)
        {
            indexUseCount = GetVertexAttribTypeIndexCount(attribType);
            size = GetVertexAttribTypeSize(attribType);
            type = GetVertexAttribBaseType(attribType);
        }

        /// <summary>
        /// Gets the base variable type for the specified attribute type
        /// (for example, vec4 would return float. dmat2 would return double, ivec2 returns int).
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
        /// Gets the attribute's size. By size, this means "vector size" (float is 1, vec2i is 2, bvec4 is 4, etc).
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
        /// Gets the amount of indices the vertex attribute occupies.
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
        /// Gets the size in bytes of an attribute type.
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
        /// Copies the given VertexAttribDescription array into a new array, but doesn't copy the descriptions that are only used for padding.
        /// </summary>
        public static VertexAttribDescription[] CopyVertexAttribDescriptionsWithoutPaddingDescriptors(VertexAttribDescription[] descriptions)
        {
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
        /// Gets the size in bytes for one element of the specified type.
        /// If the provided type isn't GL_UNSIGNED_BYTE, GL_UNSIGNED_SHORT or GL_UNSIGNED_INT, this method throws an exception.
        /// </summary>
        /// <param name="type">The type of element.</param>
        public static int GetSizeInBytesOfElementType(DrawElementsType type)
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
        /// Returns whether the given ActiveUniformType is a sampler type.
        /// </summary>
        /// <param name="type">The type of uniform to compare.</param>
        public static bool IsUniformSamplerType(ActiveUniformType type)
        {
            return (type >= ActiveUniformType.Sampler1D && type <= ActiveUniformType.Sampler2DRectShadow)
                || (type >= ActiveUniformType.Sampler1DArray && type <= ActiveUniformType.SamplerCubeShadow)
                || (type >= ActiveUniformType.IntSampler1D && type <= ActiveUniformType.UnsignedIntSamplerBuffer)
                || (type >= ActiveUniformType.SamplerCubeMapArray && type <= ActiveUniformType.UnsignedIntSamplerCubeMapArray)
                || (type >= ActiveUniformType.Sampler2DMultisample && type <= ActiveUniformType.UnsignedIntSampler2DMultisampleArray);
        }

        /// <summary>
        /// Turns a value from the TextureImageFormat enum into the necessary enums to create an OpenGL texture's image storage.
        /// </summary>
        /// <param name="imageFormat">The requested image format.</param>
        /// <param name="pixelInternalFormat">The pixel's internal format.</param>
        /// <param name="pixelType">The pixel's type.</param>
        public static void GetTextureFormatEnums(TextureImageFormat imageFormat, out PixelInternalFormat pixelInternalFormat, out PixelType pixelType, out PixelFormat pixelFormat)
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
                            pixelInternalFormat = PixelInternalFormat.Rgba8;
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
                            pixelInternalFormat = PixelInternalFormat.R32f;
                            pixelFormat = PixelFormat.Red;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32f;
                            pixelFormat = PixelFormat.Rg;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32f;
                            pixelFormat = PixelFormat.Rgb;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32f;
                            pixelFormat = PixelFormat.Rgba;
                            return;
                        case 5:
                            pixelInternalFormat = PixelInternalFormat.DepthComponent16;
                            pixelFormat = PixelFormat.DepthComponent;
                            return;
                        case 6:
                            pixelInternalFormat = PixelInternalFormat.DepthComponent24;
                            pixelFormat = PixelFormat.DepthComponent;
                            return;
                        case 7:
                            pixelInternalFormat = PixelInternalFormat.DepthComponent32f;
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
                            pixelInternalFormat = PixelInternalFormat.R32i;
                            pixelFormat = PixelFormat.RgbaInteger;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32i;
                            pixelFormat = PixelFormat.RgbaInteger;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32i;
                            pixelFormat = PixelFormat.RgbaInteger;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32i;
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
                            pixelInternalFormat = PixelInternalFormat.R32ui;
                            pixelFormat = PixelFormat.RedInteger;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32ui;
                            pixelFormat = PixelFormat.RgInteger;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32ui;
                            pixelFormat = PixelFormat.RgbInteger;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32ui;
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
                            pixelType = PixelType.UnsignedInt248;
                            pixelInternalFormat = PixelInternalFormat.Depth24Stencil8;
                            pixelFormat = PixelFormat.DepthStencil;
                            return;
                    }
                    break;
                    #endregion
            }

            throw new ArgumentException("Image format is not a valid TextureImageFormat value", "imageFormat");
        }

        /// <summary>
        /// Gets the default pixel format for get data texture operations for the specified TextureImageFormat.
        /// </summary>
        /// <param name="format">The format to check.</param>
        public static PixelFormat GetDefaultGetDataFormat(TextureImageFormat format)
        {
            if (TrippyUtils.IsImageFormatColorRenderable(format))
                return PixelFormat.Rgba;
            else if (TrippyUtils.IsImageFormatDepthType(format))
                return PixelFormat.DepthComponent;
            else if (TrippyUtils.IsImageFormatDepthStencilType(format))
                return PixelFormat.DepthStencil;
            else if (TrippyUtils.IsImageFormatStencilType(format))
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



        /// <summary>
        /// Returns the amount of components used by a specified type of transform feedback variable.
        /// </summary>
        /// <param name="type">The variable type to check.</param>
        public static int GetTransformFeedbackTypeComponentCount(TransformFeedbackType type)
        {
            if (type >= TransformFeedbackType.Int && type <= TransformFeedbackType.Float) //UnsignedInt is in between these
                return 1;

            switch (type)
            {
                case TransformFeedbackType.Double:
                case TransformFeedbackType.FloatVec2:
                case TransformFeedbackType.IntVec2:
                case TransformFeedbackType.UnsignedIntVec2:
                    return 2;

                case TransformFeedbackType.FloatVec3:
                case TransformFeedbackType.IntVec3:
                case TransformFeedbackType.UnsignedIntVec3:
                    return 3;

                case TransformFeedbackType.FloatVec4:
                case TransformFeedbackType.IntVec4:
                case TransformFeedbackType.UnsignedIntVec4:
                case TransformFeedbackType.FloatMat2:
                case TransformFeedbackType.DoubleVec2:
                    return 4;

                case TransformFeedbackType.FloatMat2x3:
                case TransformFeedbackType.FloatMat3x2:
                case TransformFeedbackType.DoubleVec3:
                    return 6;

                case TransformFeedbackType.FloatMat2x4:
                case TransformFeedbackType.FloatMat4x2:
                case TransformFeedbackType.DoubleVec4:
                case TransformFeedbackType.DoubleMat2:
                    return 8;

                case TransformFeedbackType.FloatMat3x4:
                case TransformFeedbackType.FloatMat4x3:
                case TransformFeedbackType.DoubleMat2x3:
                case TransformFeedbackType.DoubleMat3x2:
                    return 12;

                case TransformFeedbackType.FloatMat3:
                    return 9;

                case TransformFeedbackType.FloatMat4:
                case TransformFeedbackType.DoubleMat2x4:
                case TransformFeedbackType.DoubleMat4x2:
                    return 16;

                case TransformFeedbackType.DoubleMat3:
                    return 18;

                case TransformFeedbackType.DoubleMat4x3:
                case TransformFeedbackType.DoubleMat3x4:
                    return 24;

                case TransformFeedbackType.DoubleMat4:
                    return 32;
            }

            throw new ArgumentException("The specified TransformFeedbackType value is invalid", "type");
        }

        /// <summary>
        /// Returns whether the specified transform feedback variable type is a double-precition type.
        /// </summary>
        /// <param name="type">The variable type to check.</param>
        public static bool IsTransformFeedbackTypeDoublePrecition(TransformFeedbackType type)
        {
            return type == TransformFeedbackType.Double || type >= TransformFeedbackType.DoubleMat2;
        }
    }
}
