using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Inputs
{
    public static class Timers
    {
        public static void Activate(Point16 pos)
        {
            if (Main.tile[pos].TileFrameY == 0)
            {
                Main.tile[pos].TileFrameY = 18;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    WiringWrapper.CheckMech(pos.X, pos.Y, 18000);
                }
            }
            else
            {
                Main.tile[pos].TileFrameY = 0;
            }
            // SoundEngine.PlaySound(28, i * 16, j * 16, 0, 1f, 0f);
        }
    }
}