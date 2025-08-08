using Terraria;

namespace Wirelog.Outputs
{
    public static class Actuator
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ActuateForced(outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
        }
    }
}