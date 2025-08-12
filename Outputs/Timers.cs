using System.Linq;
using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Timers
    {
        public static void Activate(OutputPort outputPort)
        {
            WiringWrapper.HitSwitch(outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
            WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
            NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
        }

        public static void Postprocess(Output output)
        {
            foreach (var outputPort in output.OutputPorts)
            {
                if (outputPort.Wire.InputPorts.Any(inputPort =>
                inputPort.Inputs.Any(input =>
                input.Pos == output.Pos)))
                {
                    Link.Remove(outputPort);
                    // Two inputs cause pruning bug.
                }
            }
        }
    }
}