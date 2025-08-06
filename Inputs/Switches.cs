using Terraria;
using Terraria.Audio;

namespace Wirelog.Inputs
{
    public static class Switches
    {
        public static void Activate(Input input)
        {
            if (Main.tile[input.Pos].TileFrameY == 0)
            {
                Main.tile[input.Pos].TileFrameY = 18;
            }
            else
            {
                Main.tile[input.Pos].TileFrameY = 0;
            }
            // SoundEngine.PlaySound(28, i * 16, j * 16, 0, 1f, 0f);
        }
    }
}