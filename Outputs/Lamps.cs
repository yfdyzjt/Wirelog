using Terraria;

namespace Wirelog.Outputs
{
    public static class Lamps
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleLamp(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null, false);
        }
    }
}