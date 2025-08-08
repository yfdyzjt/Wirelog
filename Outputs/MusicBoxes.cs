using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class MusicBoxes
    {
        public static void Activate(Point16 pos)
        {
            WorldGen.SwitchMB(pos.X, pos.Y);
        }
    }
}