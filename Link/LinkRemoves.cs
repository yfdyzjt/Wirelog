using System.Collections.Generic;

namespace Wirelog
{
    public static partial class Link
    {
        public static bool Remove(Wire wire, ICollection<Gate> gates)
        {
            var result = true;
            foreach (var gate in gates)
            {
                result &= Remove(wire, gate);
            }
            return result;
        }

        public static bool Remove(Wire wire, ICollection<Lamp> lamps)
        {
            var result = true;
            foreach (var lamp in lamps)
            {
                result &= Remove(wire, lamp);
            }
            return result;
        }

        public static bool Remove(Wire wire, ICollection<OutputPort> outputPorts)
        {
            var result = true;
            foreach (var outputPort in outputPorts)
            {
                result &= Remove(wire, outputPort);
            }
            return result;
        }

        public static bool Remove(Wire wire, ICollection<InputPort> inputPorts)
        {
            var result = true;
            foreach (var inputPort in inputPorts)
            {
                result &= Remove(wire, inputPort);
            }
            return result;
        }

        public static bool Remove(ICollection<Wire> wires, Gate gate)
        {
            var result = true;
            foreach (var wire in wires)
            {
                result &= Remove(wire, gate);
            }
            return result;
        }

        public static bool Remove(ICollection<Wire> wires, Lamp lamp)
        {
            var result = true;
            foreach (var wire in wires)
            {
                result &= Remove(wire, lamp);
            }
            return result;
        }

        public static bool Remove(Output output, ICollection<OutputPort> outputPorts)
        {
            var result = true;
            foreach (var outputPort in outputPorts)
            {
                result &= Remove(output, outputPort);
            }
            return result;
        }

        public static bool Remove(ICollection<Lamp> lamps, Gate gate)
        {
            var result = true;
            foreach (var lamp in lamps)
            {
                result &= Remove(lamp, gate);
            }
            return result;
        }
    }
}
