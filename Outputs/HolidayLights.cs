using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class HolidayLights
    {
        public static void Activate(Point16 pos)
        {
            Wiring.ToggleHolidayLight(pos.X, pos.Y, Main.tile[pos], null);
        }
    }
}