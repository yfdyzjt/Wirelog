using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class TrapdoorClosed
    {
        public static void Activate(Point16 pos) => TrapdoorOpen.Activate(pos);
    }
}