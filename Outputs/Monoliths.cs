using Terraria;

namespace Wirelog.Outputs
{
    public static class Monoliths
    {
        public static void Activate(OutputPort outputPort)
        {
            WorldGen.SwitchMonolith(outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
        }
    }
}