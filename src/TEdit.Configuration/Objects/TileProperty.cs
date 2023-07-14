﻿using System.Linq;
using TEdit.Geometry;
using TEdit.Common;
using System.Collections.Generic;

namespace TEdit.Terraria.Objects
{
    public class TileProperty : ITile
    {
        public override string ToString() => Name;
        public Vector2Short TextureGrid { get; set; }
        public Vector2Short FrameGap { get; set; } = new Vector2Short(0, 0);
        public FramePlacement Placement { get; set; }
        public bool IsAnimated { get; set; }
        public bool IsLight { get; set; }
        public bool IsSolidTop { get; set; }
        public bool IsSolid { get; set; }
        public bool SaveSlope { get; set; }
        public bool HasSlopes => IsSolid || SaveSlope;
        public List<FrameProperty> Frames { get; } = new List<FrameProperty>();
        public TEditColor Color { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "UNKNOWN";
        public Vector2Short[] FrameSize { get; set; } = new Vector2Short[] { new Vector2Short(1, 1) };
        public bool IsFramed { get; set; }
        public bool IsGrass { get; set; }
        public bool IsPlatform { get; set; }
        public bool IsCactus { get; set; }
        public bool IsStone { get; set; }
        public bool CanBlend { get; set; }
        public int? MergeWith { get; set; }

        public bool Merges(int other)
        {
            if (other == this.Id) return true;

            if (!MergeWith.HasValue) return false;

            return MergeWith.Value == other;
        }

        public bool Merges(TileProperty other)
        {
            if (other.MergeWith.HasValue && other.MergeWith.Value == Id) return true;
            if (MergeWith.HasValue && MergeWith.Value == other.Id) return true;
            if (MergeWith.HasValue && other.MergeWith.HasValue && MergeWith.Value == other.MergeWith.Value) return true;

            return false;
        }

        public int GetFrameSizeIndex(short v)
        {
            if (FrameSize.Length > 1)
            {
                int row = v / TextureGrid.Y;
                int rowTest = 0;

                for (int pos = 0; pos < FrameSize.Length; pos++)
                {
                    if (row == rowTest)
                    {
                        return pos;
                    }

                    rowTest += FrameSize[pos].Y;
                }
                return FrameSize.Length - 1;
            }

            return 0;
        }

        public Vector2Short GetFrameSize(short v) => FrameSize[GetFrameSizeIndex(v)];

        public bool IsOrigin(Vector2Short uv)
        {
            if (uv == Vector2Short.Zero) return true;

            var renderUV = GetRenderUV((ushort)Id, uv.X, uv.Y);
            var frameSizeIx = GetFrameSizeIndex((short)renderUV.Y);
            var frameSize = FrameSize[frameSizeIx];

            if (frameSizeIx == 0)
            {
                return (renderUV.X % ((TextureGrid.X + FrameGap.X) * frameSize.X) == 0 &&
                        renderUV.Y % ((TextureGrid.Y + FrameGap.Y) * frameSize.Y) == 0);
            }
            else
            {
                int y = 0;
                for (int i = 0; i < frameSizeIx; i++)
                {
                    y += FrameSize[i].Y * (TextureGrid.Y + FrameGap.Y);
                }
                return (renderUV.X % ((TextureGrid.X + FrameGap.X) * frameSize.X) == 0 && renderUV.Y == y);
            }
        }


        public bool IsOrigin(Vector2Short uv, out FrameProperty frame)
        {
            frame = Frames.FirstOrDefault(f => f.UV == uv);

            return frame != null;
        }


        public static Vector2Int32 GetRenderUV(ushort type, short U, short V)
        {
            int renderU = U;
            int renderV = V;

            switch (type)
            {
                case 87:
                case 88:
                case 89:
                    {
                        int u = U / 1998;
                        renderU -= 1998 * u;
                        renderV += 36 * u;
                    }
                    break;
                case 93:
                    {
                        int v = V / 1998;
                        renderU += 36 * v;
                        renderV -= 1998 * v;
                    }
                    break;
                case 101:
                    {
                        int u = U / 1998;
                        renderU -= 1998 * u;
                        renderV += 72 * u;
                    }
                    break;
                case 185:
                    if (V == 18)
                    {
                        int u = U / 1908;
                        renderU -= 1908 * u;
                        renderV += 18 * u;
                    }
                    break;
                case 187:
                    {
                        int u = U / 1890;
                        renderU -= 1890 * u;
                        renderV += 36 * u;
                    }
                    break;
            }

            return new Vector2Int32(renderU, renderV);
        }
    }
}