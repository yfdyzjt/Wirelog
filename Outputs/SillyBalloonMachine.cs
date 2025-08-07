using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class SillyBalloonMachine
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num2 = tile.TileFrameX % 54 / 18;
            int num3 = tile.TileFrameY % 54 / 18;
            int num4 = pos.X - num2;
            int num5 = pos.Y - num3;
            int num6 = 54;
            if (Main.tile[num4, num5].TileFrameY >= 54)
            {
                num6 = -54;
            }
            for (int k = num4; k < num4 + 3; k++)
            {
                for (int l = num5; l < num5 + 3; l++)
                {
                    Main.tile[k, l].TileFrameX = (short)(Main.tile[k, l].TileFrameX + num6);
                }
            }
            NetMessage.SendTileSquare(-1, num4 + 1, num5 + 1, 3, TileChangeType.None);
        }
    }
}