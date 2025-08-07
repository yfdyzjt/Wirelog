using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class TallGates
    {
        public static void Activate(Point16 pos)
        {
            bool flag4 = Main.tile[pos].TileType == 389;
            WorldGen.ShiftTallGate(pos.X, pos.Y, flag4, false);
            NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 4 + flag4.ToInt(), pos.X, pos.Y, 0f, 0, 0, 0);
        }
    }
}