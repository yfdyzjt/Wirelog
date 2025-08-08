using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class MinecartTrack
    {
        public static void Activate(Point16 pos)
        {
            if (WiringWrapper.CheckMech(pos.X, pos.Y, 5))
            {
                Minecart.FlipSwitchTrack(pos.X, pos.Y);
                return;
            }
        }
    }
}