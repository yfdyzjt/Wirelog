using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Timers
    {
        public static void Activate(OutputPort outputPort)
        {
            WiringWrapper.HitSwitch(outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
            WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
            NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
        }
    }
}