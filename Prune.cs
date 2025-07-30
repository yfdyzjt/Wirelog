using System.Collections.Generic;
using System.Linq;

namespace Wirelog
{
    public partial class Converter
    {
        private static void Prune()
        {
            bool changed = false;
            do
            {
                changed |= PruneUnusedWires();
                changed |= PruneUnusedGatesAndLamps();
            } while (changed);
        }

        private static bool PruneUnusedWires()
        {
            var wiresToRemove = _wires.Where(w => w.OutputPorts.Count == 0 && w.Lamps.Count == 0).ToList();

            if (wiresToRemove.Count == 0) return false;

            foreach (var wire in wiresToRemove)
            {
                foreach (var gate in wire.Gates)
                    gate.OutputWires.Remove(wire);
                foreach (var inputPort in wire.InputPorts)
                    inputPort.OutputWires.Remove(wire);
            }

            _wires.RemoveAll(wiresToRemove.Contains);
            return true;
        }
    }
}
