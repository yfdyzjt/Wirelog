namespace Wirelog.Inputs
{
    public static class LogicSensor
    {
        public static void Activate(Input input) => PressurePlates.Activate(input);
    }
}