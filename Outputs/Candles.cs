using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Candles
    {
        public static void Activate(Point16 pos)
        {
            Wiring.ToggleCandle(pos.X, pos.Y, Main.tile[pos], null);
        }
    }
}