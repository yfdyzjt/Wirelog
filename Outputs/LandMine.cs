using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class LandMine
    {
        public static void Activate(Point16 pos)
        {
            WorldGen.ExplodeMine(pos.X, pos.Y, true);
        }
    }
}