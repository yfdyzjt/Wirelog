using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Torches
    {
        public static void Activate(Point16 pos)
        {
            if (Main.tile[pos].TileFrameX < 66)
            {
                Main.tile[pos].TileFrameX += 66;
            }
            else
            {
                Main.tile[pos].TileFrameX -= 66;
            }
            NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
        }
    }
}