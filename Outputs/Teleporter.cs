namespace Wirelog.Outputs
{
    public static class Teleporter
    {
        public static void Activate(OutputPort outputPort)
        {
        }

        public static void Postprocess(Output output)
        {
            foreach(var outputPort in output.OutputPorts)
            {
                foreach(var inputPort in outputPort.InputWire.InputPorts)
                {

                }
                foreach(var gate in outputPort.InputWire.Gates)
                {

                }
            }
        }
    }
}