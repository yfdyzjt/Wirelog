using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class PressurePlateTrack
    {
        public static void Activate(Point16 pos) => PressurePlates.Activate(pos);
    }
}