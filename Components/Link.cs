using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wirelog
{
    public static class Link
    {
        public static bool Add(Wire wire, Gate gate)
        {
            return wire.Gates.Add(gate) && gate.OutputWires.Add(wire);
        }
    }
}
