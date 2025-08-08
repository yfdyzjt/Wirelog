using Terraria;

namespace Wirelog.Outputs
{
    public static class HolidayLights
    {
        public static void Activate(OutputPort outputPort)
        {
            Wiring.ToggleHolidayLight(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, Main.tile[outputPort.Output.Pos], null);
        }
    }
}