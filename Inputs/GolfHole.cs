using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class GolfHole
    {
        public static void Activate(Point16 pos) => PressurePlates.Activate(pos);
    }
}