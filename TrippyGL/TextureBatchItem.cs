using System;
using System.Drawing;
using System.Numerics;

namespace TrippyGL
{
    /// <summary>
    /// Used internally by <see cref="TextureBatcher"/> to store the vertices for each Draw().
    /// </summary>
    internal class TextureBatchItem : IComparable<TextureBatchItem>
    {
        /// <summary>The <see cref="Texture2D"/> to draw the vertices with.</summary>
        public Texture2D Texture;

        /// <summary>
        /// A value used for sorting. It's value might come from different places depending
        /// on the <see cref="TextureBatcher.BeginMode"/>.
        /// </summary>
        public float SortValue;

        /// <summary>The top-left vertex.</summary>
        public VertexColorTexture VertexTL;
        /// <summary>The top-right vertex.</summary>
        public VertexColorTexture VertexTR;
        /// <summary>The bottom-left vertex.</summary>
        public VertexColorTexture VertexBL;
        /// <summary>The bottom-right vertex.</summary>
        public VertexColorTexture VertexBR;

        /// <summary>
        /// Calculates and sets all the values in this <see cref="TextureBatchItem"/> except for <see cref="SortValue"/>.
        /// </summary>
        public void SetValue(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, Vector2 scale, float rotation, Vector2 origin, float depth)
        {
            Texture = texture;
            Rectangle sourceRect = source ?? new Rectangle(0, 0, (int)texture.Width, (int)texture.Height);

            Vector2 tl = -origin * scale;
            Vector2 tr = new Vector2(tl.X + sourceRect.Width * scale.X, tl.Y);
            Vector2 bl = new Vector2(tl.X, tl.Y + sourceRect.Height * scale.Y);
            Vector2 br = new Vector2(tr.X, bl.Y);

            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            VertexTL.Position = new Vector3(cos * tl.X - sin * tl.Y + position.X, sin * tl.X + cos * tl.Y + position.Y, depth);
            VertexTR.Position = new Vector3(cos * tr.X - sin * tr.Y + position.X, sin * tr.X + cos * tr.Y + position.Y, depth);
            VertexBL.Position = new Vector3(cos * bl.X - sin * bl.Y + position.X, sin * bl.X + cos * bl.Y + position.Y, depth);
            VertexBR.Position = new Vector3(cos * br.X - sin * br.Y + position.X, sin * br.X + cos * br.Y + position.Y, depth);

            VertexTL.TexCoords = new Vector2(sourceRect.X / (float)texture.Width, sourceRect.Y / (float)texture.Height);
            VertexBR.TexCoords = new Vector2(sourceRect.Right / (float)texture.Width, sourceRect.Bottom / (float)texture.Height);
            VertexTR.TexCoords = new Vector2(VertexBR.TexCoords.X, VertexTL.TexCoords.Y);
            VertexBL.TexCoords = new Vector2(VertexTL.TexCoords.X, VertexBR.TexCoords.Y);

            VertexTL.Color = color;
            VertexTR.Color = color;
            VertexBL.Color = color;
            VertexBR.Color = color;
        }

        /// <summary>
        /// Calculates and sets all the values in this <see cref="TextureBatchItem"/> except for <see cref="SortValue"/>.
        /// </summary>
        public void SetValue(Texture2D texture, Vector2 position, Rectangle source, Color4b color, Vector2 scale, float sin, float cos, float depth)
        {
            Texture = texture;

            Vector2 size = new Vector2(source.Width, source.Height) * scale;

            VertexTL.Position = new Vector3(position.X, position.Y, depth);
            VertexTR.Position = new Vector3(cos * size.X + position.X, sin * size.X + position.Y, depth);
            VertexBL.Position = new Vector3(-sin * size.Y + position.X, cos * size.Y + position.Y, depth);
            VertexBR.Position = new Vector3(cos * size.X - sin * size.Y + position.X, sin * size.X + cos * size.Y + position.Y, depth);

            VertexTL.TexCoords = new Vector2(source.X / (float)texture.Width, source.Y / (float)texture.Height);
            VertexBR.TexCoords = new Vector2(source.Right / (float)texture.Width, source.Bottom / (float)texture.Height);
            VertexTR.TexCoords = new Vector2(VertexBR.TexCoords.X, VertexTL.TexCoords.Y);
            VertexBL.TexCoords = new Vector2(VertexTL.TexCoords.X, VertexBR.TexCoords.Y);

            VertexTL.Color = color;
            VertexTR.Color = color;
            VertexBL.Color = color;
            VertexBR.Color = color;
        }

        /// <summary>
        /// Calculates and sets all the values in this <see cref="TextureBatchItem"/> except for <see cref="SortValue"/>
        /// without calculating rotation.
        /// </summary>
        public void SetValue(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, Vector2 scale, Vector2 origin, float depth)
        {
            Texture = texture;
            Rectangle sourceRect = source ?? new Rectangle(0, 0, (int)texture.Width, (int)texture.Height);

            Vector2 tl = position - origin * scale;
            Vector2 br = tl + new Vector2(sourceRect.Width, sourceRect.Height) * scale;
            VertexTL.Position = new Vector3(tl, depth);
            VertexTR.Position = new Vector3(br.X, tl.Y, depth);
            VertexBL.Position = new Vector3(tl.X, br.Y, depth);
            VertexBR.Position = new Vector3(br, depth);

            VertexTL.TexCoords = new Vector2(sourceRect.X / (float)texture.Width, sourceRect.Y / (float)texture.Height);
            VertexBR.TexCoords = new Vector2(sourceRect.Right / (float)texture.Width, sourceRect.Bottom / (float)texture.Height);
            VertexTR.TexCoords = new Vector2(VertexBR.TexCoords.X, VertexTL.TexCoords.Y);
            VertexBL.TexCoords = new Vector2(VertexTL.TexCoords.X, VertexBR.TexCoords.Y);

            VertexTL.Color = color;
            VertexTR.Color = color;
            VertexBL.Color = color;
            VertexBR.Color = color;
        }

        /// <summary>
        /// Calculates and sets all the values in this <see cref="TextureBatchItem"/> except for <see cref="SortValue"/>
        /// without calculating rotation.
        /// </summary>
        public void SetValue(Texture2D texture, Vector2 position, Rectangle source, Color4b color, Vector2 scale, float depth)
        {
            Texture = texture;

            Vector2 size = new Vector2(source.Width, source.Height) * scale;
            VertexTL.Position = new Vector3(position, depth);
            VertexTR.Position = new Vector3(position.X + size.X, position.Y, depth);
            VertexBL.Position = new Vector3(position.X, position.Y + size.Y, depth);
            VertexBR.Position = new Vector3(position + size, depth);

            VertexTL.TexCoords = new Vector2(source.X / (float)texture.Width, source.Y / (float)texture.Height);
            VertexBR.TexCoords = new Vector2(source.Right / (float)texture.Width, source.Bottom / (float)texture.Height);
            VertexTR.TexCoords = new Vector2(VertexBR.TexCoords.X, VertexTL.TexCoords.Y);
            VertexBL.TexCoords = new Vector2(VertexTL.TexCoords.X, VertexBR.TexCoords.Y);

            VertexTL.Color = color;
            VertexTR.Color = color;
            VertexBL.Color = color;
            VertexBR.Color = color;
        }

        /// <summary>
        /// Calculates and sets all the values on this <see cref="TextureBatchItem"/> except for <see cref="SortValue"/>
        /// without calculating rotation nor scale.
        /// </summary>
        public void SetValue(Texture2D texture, Vector2 position, Rectangle source, Color4b color, float depth)
        {
            Texture = texture;

            VertexTL.Position = new Vector3(position, depth);
            VertexTR.Position = new Vector3(position.X + source.Width, position.Y, depth);
            VertexBL.Position = new Vector3(position.X, position.Y + source.Height, depth);
            VertexBR.Position = new Vector3(position + new Vector2(source.Width, source.Height), depth);

            VertexTL.TexCoords = new Vector2(source.X / (float)texture.Width, source.Y / (float)texture.Height);
            VertexBR.TexCoords = new Vector2(source.Right / (float)texture.Width, source.Bottom / (float)texture.Height);
            VertexTR.TexCoords = new Vector2(VertexBR.TexCoords.X, VertexTL.TexCoords.Y);
            VertexBL.TexCoords = new Vector2(VertexTL.TexCoords.X, VertexBR.TexCoords.Y);

            VertexTL.Color = color;
            VertexTR.Color = color;
            VertexBL.Color = color;
            VertexBR.Color = color;
        }

        /// <summary>
        /// Compares this item's <see cref="SortValue"/> with another item's.
        /// </summary>
        public int CompareTo(TextureBatchItem other)
        {
            return SortValue.CompareTo(other.SortValue);
        }

        public override string ToString()
        {
            return Texture == null ? "Empty " + nameof(TextureBatchItem)
                : string.Concat("Texture.Handle=", Texture.Handle.ToString(), ", " + nameof(SortValue) + "=", SortValue.ToString());
        }
    }
}
