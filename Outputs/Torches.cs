using Terraria;

namespace Wirelog.Outputs
{
    public static class Torches
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleTorch(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null);
        }
    }
}