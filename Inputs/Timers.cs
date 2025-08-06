using Terraria;
using Terraria.ID;

namespace Wirelog.Inputs
{
    public static class Timers
    {
        public static void Activate(Input input)
        {
            if (Main.tile[input.Pos].TileFrameY == 0)
            {
                Main.tile[input.Pos].TileFrameY = 18;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    WiringWrapper.CheckMech(input.Pos.X, input.Pos.Y, 18000);
                }
            }
            else
            {
                Main.tile[input.Pos].TileFrameY = 0;
            }
            // SoundEngine.PlaySound(28, i * 16, j * 16, 0, 1f, 0f);
        }
    }
}