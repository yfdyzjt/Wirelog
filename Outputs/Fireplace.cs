using Terraria;

namespace Wirelog.Outputs
{
    public static class Fireplace
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleFirePlace(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null, false);
        }
    }
}