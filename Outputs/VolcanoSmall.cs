using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class VolcanoSmall
    {
        public static void Activate(Point16 pos)
        {
            short num93;
            if (Main.tile[pos].TileFrameX == 0)
            {
                num93 = 18;
            }
            else
            {
                num93 = -18;
            }
            Main.tile[pos].TileFrameX += num93;
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, pos.X, pos.Y, 1, 1, TileChangeType.None);
            }
            int num94 = (num93 > 0) ? 4 : 3;
            Animation.NewTemporaryAnimation(num94, 593, pos.X, pos.Y);
            NetMessage.SendTemporaryAnimation(-1, num94, 593, pos.X, pos.Y);
        }
    }
}