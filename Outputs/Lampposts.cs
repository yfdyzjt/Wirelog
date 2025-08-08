using Terraria;

namespace Wirelog.Outputs
{
    public static class Lampposts
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleLampPost(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null, false);
        }
    }
}