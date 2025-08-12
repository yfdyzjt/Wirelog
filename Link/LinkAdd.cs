namespace Wirelog
{
    public static partial class Link
    {
        public static bool Add(Wire wire, Gate gate)
        {
            return wire?.Gates.Add(gate) == true && gate?.Wires.Add(wire) == true;
        }

        public static bool Add(Wire wire, Lamp lamp)
        {
            return wire?.Lamps.Add(lamp) == true && lamp?.Wires.Add(wire) == true;
        }

        public static bool Add(Wire wire, OutputPort outputPort)
        {
            var isNull = outputPort?.Wire == null;
            if (outputPort != null) outputPort.Wire = wire;
            return wire?.OutputPorts.Add(outputPort) == true && isNull;
        }

        public static bool Add(Wire wire, InputPort inputPort)
        {
            return wire?.InputPorts.Add(inputPort) == true && inputPort?.Wires.Add(wire) == true;
        }

        public static bool Add(Output output, OutputPort outputPort)
        {
            var isNull = outputPort?.Output == null;
            if (outputPort != null) outputPort.Output = output;
            return output?.OutputPorts.Add(outputPort) == true && isNull;
        }

        public static bool Add(Input input, InputPort inputPort)
        {
            var isNull = input?.InputPort == null;
            if (input != null) input.InputPort = inputPort;
            return inputPort?.Inputs.Add(input) == true && isNull;
        }

        public static bool Add(Lamp lamp, Gate gate)
        {
            var isNull = lamp?.Gate == null;
            if (lamp != null) lamp.Gate = gate;
            return gate?.Lamps.Add(lamp) == true && isNull;
        }
    }
}
