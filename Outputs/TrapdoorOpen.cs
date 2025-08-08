using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class TrapdoorOpen
    {
        public static void Activate(OutputPort outputPort)
        {
            var newPos = outputPort.Output.Pos;
            for (int i = -1; i <= 1; i++)
            {
                if (Main.tile[outputPort.Output.Pos.X, outputPort.Output.Pos.Y + i].TileType is 386 or 387)
                {
                    newPos = new Point16(outputPort.Output.Pos.X, outputPort.Output.Pos.Y + i);
                    break;
                }
            }

            if(Main.tile[newPos].TileType is 386 or 387)
            {
                bool value = Main.tile[newPos].TileType == 387;
                int num66 = WorldGen.ShiftTrapdoor(newPos.X, newPos.Y, true, -1).ToInt();
                if (num66 == 0)
                {
                    num66 = -WorldGen.ShiftTrapdoor(newPos.X, newPos.Y, false, -1).ToInt();
                }
                if (num66 != 0)
                {
                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 3 - value.ToInt(), newPos.X, newPos.Y, num66, 0, 0, 0);
                    return;
                }
            }
        }
    }
}