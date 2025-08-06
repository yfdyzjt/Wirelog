using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class LogicSensor
    {
        public static void Activate(Point16 pos) => PressurePlates.Activate(pos);
    }
}