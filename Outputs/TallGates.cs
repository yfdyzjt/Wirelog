using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class TallGates
    {
        public static void Activate(OutputPort outputPort)
        {
            bool flag4 = Main.tile[outputPort.Output.Pos].TileType == 389;
            WorldGen.ShiftTallGate(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, flag4, false);
            NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 4 + flag4.ToInt(), outputPort.Output.Pos.X, outputPort.Output.Pos.Y, 0f, 0, 0, 0);
        }
    }
}