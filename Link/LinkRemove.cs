namespace Wirelog
{
    public static partial class Link
    {
        public static bool Remove(Wire wire, Gate gate)
        {
            return wire?.Gates.Remove(gate) == true && gate?.Wires.Remove(wire) == true;
        }

        public static bool Remove(Wire wire, Lamp lamp)
        {
            return wire?.Lamps.Remove(lamp) == true && lamp?.Wires.Remove(wire) == true;
        }

        public static bool Remove(Wire wire, OutputPort outputPort)
        {
            var isNotNull = outputPort?.Wire == wire;
            if (outputPort != null) outputPort.Wire = null;
            return wire?.OutputPorts.Remove(outputPort) == true && isNotNull;
        }

        public static bool Remove(Wire wire, InputPort inputPort)
        {
            return wire?.InputPorts.Remove(inputPort) == true && inputPort?.Wires.Remove(wire) == true;
        }

        public static bool Remove(Output output, OutputPort outputPort)
        {
            var isNotNull = outputPort?.Output == output;
            if (outputPort != null) outputPort.Output = null;
            return output?.OutputPorts.Remove(outputPort) == true && isNotNull;
        }

        public static bool Remove(Input input, InputPort inputPort)
        {
            var isNotNull = input?.InputPort == inputPort;
            if (input != null) input.InputPort = null;
            return inputPort?.Inputs.Remove(input) == true && isNotNull;
        }

        public static bool Remove(Lamp lamp, Gate gate)
        {
            var isNotNull = lamp?.Gate == gate;
            if (lamp != null) lamp.Gate = null;
            return gate?.Lamps.Remove(lamp) == true && isNotNull;
        }
    }
}
