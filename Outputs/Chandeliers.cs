using Terraria;

namespace Wirelog.Outputs
{
    public static class Chandeliers
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleChandelier(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null, false);
        }
    }
}