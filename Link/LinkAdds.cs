using System.Collections.Generic;

namespace Wirelog
{
    public static partial class Link
    {
        public static bool Add(IEnumerable<Wire> wires, Lamp lamp)
        {
            var result = true;
            foreach (var wire in wires)
            {
                result &= Add(wire, lamp);
            }
            return result;
        }

        public static bool Add(IEnumerable<Lamp> lamps, Gate gate)
        {
            var result = true;
            foreach (var lamp in lamps)
            {
                result &= Add(lamp, gate);
            }
            return result;
        }
    }
}
