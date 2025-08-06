using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class DeadMansChest
    {
        public static void Activate(Point16 pos) => FakeContainers.Activate(pos);
    }
}