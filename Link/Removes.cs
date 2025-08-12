using System.Collections.Generic;

namespace Wirelog
{
    public static partial class Link
    {
        public static bool Remove(IEnumerable<Input> inputs)
        {
            var result = true;
            foreach (var input in inputs)
            {
                result &= Remove(input);
            }
            return result;
        }

        public static bool Remove(IEnumerable<Output> outputs)
        {
            var result = true;
            foreach (var output in outputs)
            {
                result &= Remove(output);
            }
            return result;
        }

        public static bool Remove(IEnumerable<OutputPort> outputPorts)
        {
            var result = true;
            foreach (var outputPort in outputPorts)
            {
                result &= Remove(outputPort);
            }
            return result;
        }

        public static bool Remove(IEnumerable<Gate> gates)
        {
            var result = true;
            foreach (var gate in gates)
            {
                result &= Remove(gate);
            }
            return result;
        }

        public static bool Remove(IEnumerable<Lamp> lamps)
        {
            var result = true;
            foreach (var lamp in lamps)
            {
                result &= Remove(lamp);
            }
            return result;
        }

        public static bool Remove(IEnumerable<Wire> wires)
        {
            var result = true;
            foreach (var wire in wires)
            {
                result &= Remove(wire);
            }
            return result;
        }
    }
}
