using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog
{
    public class JunctionBox
    {
        public static bool HasJunction(Tile tile)
        {
            return tile.TileType == TileID.WirePipe || tile.TileType == TileID.PixelBox;
        }
        public static bool TryGetType(Tile tile, out JunctionBoxType type)
        {
            if (tile == null || !tile.HasTile) { type = JunctionBoxType.None; return false; }
            type = tile.TileType switch
            {
                TileID.PixelBox => JunctionBoxType.UpDown,
                TileID.WirePipe when tile.TileFrameX is 0 * 18 => JunctionBoxType.UpDown,
                TileID.WirePipe when tile.TileFrameX is 1 * 18 => JunctionBoxType.UpLeft,
                TileID.WirePipe when tile.TileFrameX is 2 * 18 => JunctionBoxType.UpRight,
                _ => JunctionBoxType.None,
            };
            return type != JunctionBoxType.None;
        }

    }

    public enum JunctionBoxType
    {
        None,
        UpDown,
        UpLeft,
        UpRight,
    }
}