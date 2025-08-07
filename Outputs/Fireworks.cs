using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Fireworks
    {
        public static void Activate(Point16 pos)
        {
            WorldGen.LaunchRocket(pos.X, pos.Y, true);
        }
    }
}