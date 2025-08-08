using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class WaterFountain
    {
        public static void Activate(Point16 pos)
        {
            WorldGen.SwitchFountain(pos.X, pos.Y);
        }
    }
}