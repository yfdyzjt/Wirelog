using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Monoliths
    {
        public static void Activate(Point16 pos)
        {
            WorldGen.SwitchMonolith(pos.X, pos.Y);
        }
    }
}