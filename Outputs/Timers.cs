using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Timers
    {
        public static void Activate(Point16 pos)
        {
            WiringWrapper.HitSwitch(pos.X, pos.Y);
            WorldGen.SquareTileFrame(pos.X, pos.Y, true);
            NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
        }
    }
}