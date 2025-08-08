namespace Wirelog.Outputs
{
    public static class ClosedDoors
    {
        public static void Activate(OutputPort outputPort) => OpenDoors.Activate(outputPort);
    }
}