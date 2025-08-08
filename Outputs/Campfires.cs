using Terraria;

namespace Wirelog.Outputs
{
    public static class Campfires
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleCampFire(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null, false);
        }
    }
}