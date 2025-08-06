using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class Switches
    {
        public static void Activate(Point16 pos)
        {
            if (Main.tile[pos].TileFrameY == 0)
            {
                Main.tile[pos].TileFrameY = 18;
            }
            else
            {
                Main.tile[pos].TileFrameY = 0;
            }
            // SoundEngine.PlaySound(28, i * 16, j * 16, 0, 1f, 0f);
            Interface.InputActivate(pos);
        }
    }
}