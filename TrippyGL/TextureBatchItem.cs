using System;
using System.Drawing;
using System.Numerics;

namespace TrippyGL
{
    internal class TextureBatchItem : IComparable<TextureBatchItem>
    {
        public Texture2D Texture;
        public float SortValue;

        public VertexColorTexture VertexTL;
        public VertexColorTexture VertexTR;
        public VertexColorTexture VertexBL;
        public VertexColorTexture VertexBR;

        public void SetValue(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, Vector2 scale, float rotation, Vector2 origin, float depth)
        {
            Texture = texture;
            Rectangle sourceRect = source.HasValue ? source.GetValueOrDefault() : new Rectangle(0, 0, (int)texture.Width, (int)texture.Height);

            Vector2 size = new Vector2(sourceRect.Width, sourceRect.Height) * scale;
            Vector2 tl = -origin * size;
            Vector2 tr = new Vector2(tl.X + size.X, tl.Y);
            Vector2 bl = new Vector2(tl.X, tl.Y + size.Y);
            Vector2 br = new Vector2(tr.X, bl.Y);

            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            VertexTL.Position = new Vector3(cos * tl.X + sin * tl.Y + position.X, -sin * tl.X + cos * tl.Y + position.Y, depth);
            VertexTR.Position = new Vector3(cos * tr.X + sin * tr.Y + position.X, -sin * tr.X + cos * tr.Y + position.Y, depth);
            VertexBL.Position = new Vector3(cos * bl.X + sin * bl.Y + position.X, -sin * bl.X + cos * bl.Y + position.Y, depth);
            VertexBR.Position = new Vector3(cos * br.X + sin * br.Y + position.X, -sin * br.X + cos * br.Y + position.Y, depth);

            VertexTL.TexCoords = new Vector2(sourceRect.X / (float)texture.Width, sourceRect.Y / (float)texture.Height);
            VertexBR.TexCoords = new Vector2(sourceRect.Right / (float)texture.Width, sourceRect.Bottom / (float)texture.Height);
            VertexTR.TexCoords = new Vector2(VertexBR.TexCoords.X, VertexTL.TexCoords.Y);
            VertexBL.TexCoords = new Vector2(VertexTL.TexCoords.X, VertexBR.TexCoords.Y);

            VertexTL.Color = color;
            VertexTR.Color = color;
            VertexBL.Color = color;
            VertexBR.Color = color;
        }

        public void SetValue(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, Vector2 scale, Vector2 origin, float depth)
        {
            Texture = texture;
            Rectangle sourceRect = source.HasValue ? source.GetValueOrDefault() : new Rectangle(0, 0, (int)texture.Width, (int)texture.Height);

            Vector2 size = new Vector2(sourceRect.Width, sourceRect.Height) * scale;
            Vector2 tl = position - origin * size;
            VertexTL.Position = new Vector3(tl, depth);
            VertexTR.Position = new Vector3(tl.X + size.X, tl.Y, depth);
            VertexBL.Position = new Vector3(tl.X, tl.Y + size.Y, depth);
            VertexBR.Position = new Vector3(tl + size, depth);

            VertexTL.TexCoords = new Vector2(sourceRect.X / (float)texture.Width, sourceRect.Y / (float)texture.Height);
            VertexBR.TexCoords = new Vector2(sourceRect.Right / (float)texture.Width, sourceRect.Bottom / (float)texture.Height);
            VertexTR.TexCoords = new Vector2(VertexBR.TexCoords.X, VertexTL.TexCoords.Y);
            VertexBL.TexCoords = new Vector2(VertexTL.TexCoords.X, VertexBR.TexCoords.Y);

            VertexTL.Color = color;
            VertexTR.Color = color;
            VertexBL.Color = color;
            VertexBR.Color = color;
        }

        public void AddToBatcher(PrimitiveBatcher<VertexColorTexture> primitiveBatcher)
        {
            primitiveBatcher.AddQuad(VertexTL, VertexTR, VertexBR, VertexBL);
        }

        public int CompareTo(TextureBatchItem other)
        {
            return SortValue.CompareTo(other.SortValue);
        }
    }
}
