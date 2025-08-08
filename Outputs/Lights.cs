using Terraria;

namespace Wirelog.Outputs
{
    public static class Lights
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.Toggle2x2Light(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null, false);
        }
    }
}