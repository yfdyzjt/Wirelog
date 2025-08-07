using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class ClosedDoors
    {
        public static void Activate(Point16 pos) => OpenDoors.Activate(pos);
    }
}