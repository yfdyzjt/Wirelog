using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Fireplace
    {
        public static void Activate(Point16 pos)
        {
            Wiring.ToggleFirePlace(pos.X, pos.Y, Main.tile[pos], null, true);
        }
    }
}