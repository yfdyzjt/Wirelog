using Terraria;

namespace Wirelog.Outputs
{
    public static class WaterFountain
    {
        public static void Activate(OutputPort outputPort)
        {
            WorldGen.SwitchFountain(outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
        }
    }
}