using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Grates
    {
        public static void Activate(Point16 pos)
        {
            if (Main.tile[pos].TileType == 546)
            {
                Main.tile[pos].TileType = 557;
            }
            else
            {
                Main.tile[pos].TileType = 546;
            }
            WorldGen.SquareTileFrame(pos.X, pos.Y, true);
            NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
        }
    }
}