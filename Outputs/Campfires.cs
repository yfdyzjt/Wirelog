using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Campfires
    {
        public static void Activate(Point16 pos)
        {
            Wiring.ToggleCampFire(pos.X, pos.Y, Main.tile[pos], null, true);
        }
    }
}