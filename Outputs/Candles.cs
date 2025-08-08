using Terraria;

namespace Wirelog.Outputs
{
    public static class Candles
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleCandle(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null);
        }
    }
}