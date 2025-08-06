using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Torches
    {
        public static void Activate(Point16 pos)
        {
            Main.NewText($"Hit torch at {pos}");
        }
    }
}