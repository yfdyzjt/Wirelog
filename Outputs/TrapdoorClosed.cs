namespace Wirelog.Outputs
{
    public static class TrapdoorClosed
    {
        public static void Activate(OutputPort outputPort) => TrapdoorOpen.Activate(outputPort);
    }
}