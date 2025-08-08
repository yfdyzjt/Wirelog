using Terraria;

namespace Wirelog.Outputs
{
    public static class MinecartTrack
    {
        public static void Activate(OutputPort outputPort)
        {
            if (WiringWrapper.CheckMech(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, 5))
            {
                Minecart.FlipSwitchTrack(outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
                return;
            }
        }
    }
}