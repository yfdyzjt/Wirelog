using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Inputs
{
    public static class Lever
    {
        public static void Activate(Point16 pos)
        {
            short num5 = 36;
            int num6 = Main.tile[pos].TileFrameX / 18 * -1;
            int num7 = Main.tile[pos].TileFrameY / 18 * -1;
            num6 %= 4;
            if (num6 < -1)
            {
                num6 += 2;
                num5 = -36;
            }
            num6 += pos.X;
            num7 += pos.Y;
            if (Main.netMode != NetmodeID.MultiplayerClient && Main.tile[num6, num7].TileType == 411)
            {
                Wiring.CheckMech(num6, num7, 60);
            }
            for (int k = num6; k < num6 + 2; k++)
            {
                for (int l = num7; l < num7 + 2; l++)
                {
                    if (Main.tile[k, l].TileType == 132 || Main.tile[k, l].TileType == 411)
                    {
                        Tile tile = Main.tile[k, l];
                        tile.TileFrameX += num5;
                    }
                }
            }
            WorldGen.TileFrame(num6, num7, false, false);
            // SoundEngine.PlaySound(28, i * 16, j * 16, 0, 1f, 0f);
            Interface.InputActivate(new Point16(num6, num7));
        }
    }
}