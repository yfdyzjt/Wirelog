using Terraria;

namespace Wirelog.Outputs
{
    public static class Fireworks
    {
        public static void Activate(OutputPort outputPort)
        {
            WorldGen.LaunchRocket(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
        }
    }
}