using Terraria;

namespace Wirelog.Outputs
{
    public static class LandMine
    {
        public static void Activate(OutputPort outputPort)
        {
            WorldGen.ExplodeMine(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
        }
    }
}