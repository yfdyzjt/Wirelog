using Terraria;

namespace Wirelog.Outputs
{
    public static class MusicBoxes
    {
        public static void Activate(OutputPort outputPort)
        {
            WorldGen.SwitchMB(outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
        }
    }
}