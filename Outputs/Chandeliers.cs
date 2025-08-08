using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Chandeliers
    {
        public static void Activate(Point16 pos)
        {
            Wiring.ToggleChandelier(pos.X, pos.Y, Main.tile[pos], null, false);
        }
    }
}