using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Gemsparks
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            if (tile.TileType >= 262)
            {
                tile.TileType -= 7;
            }
            else
            {
                tile.TileType += 7;
            }
            WorldGen.SquareTileFrame(pos.X, pos.Y, true);
            NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
        }
    }
}