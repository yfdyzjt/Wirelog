using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Lights
    {
        public static void Activate(Point16 pos)
        {
            Wiring.Toggle2x2Light(pos.X, pos.Y, Main.tile[pos], null, false);
        }
    }
}