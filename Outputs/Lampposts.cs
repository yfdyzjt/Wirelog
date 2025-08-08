using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Lampposts
    {
        public static void Activate(Point16 pos)
        {
            Wiring.ToggleLampPost(pos.X, pos.Y, Main.tile[pos], null, false);
        }
    }
}