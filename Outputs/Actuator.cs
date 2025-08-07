using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Actuator
    {
        public static void Activate(Point16 pos)
        {
            Wiring.ActuateForced(pos.X, pos.Y);
        }
    }
}