namespace Wirelog
{
    public static partial class Link
    {
        public static bool Remove(Module module)
        {
            module.Wires.Clear();
            module.InputPorts.Clear();
            module.OutputPorts.Clear();
            module.Lamps.Clear();
            module.Gates.Clear();
            return true;
        }

        public static bool Remove(ModuleInstance moduleInstance)
        {
            moduleInstance.InputMapping.Clear();
            moduleInstance.OutputMapping.Clear();
            return true;
        }

        public static bool Remove(Input input)
        {
            return Remove(input, input.InputPort);
        }

        public static bool Remove(Output output)
        {
            return Remove(output, output.OutputPorts);
        }

        public static bool Remove(OutputPort outputPort)
        {
            return Remove(outputPort.Wire, outputPort) && Remove(outputPort.Output, outputPort);
        }

        public static bool Remove(Gate gate)
        {
            return Remove(gate.Wires, gate) && Remove(gate.Lamps, gate);
        }

        public static bool Remove(Lamp lamp)
        {
            return Remove(lamp.Wires, lamp) && Remove(lamp, lamp.Gate);
        }

        public static bool Remove(Wire wire)
        {
            return Remove(wire, wire.Gates) &&
                Remove(wire, wire.Lamps) &&
                Remove(wire, wire.OutputPorts) &&
                Remove(wire, wire.InputPorts);
        }
    }
}
