using System.Linq;

namespace Wirelog
{
    public partial class Converter
    {
        private static void Prune()
        {
            PruneFaultLamps();
            PruneUnusedComponents();
        }

        private static void PruneFaultLamps()
        {
            var faultGates = _gatesFound.Where(kv => kv.Value.Type == GateType.Fault);
            foreach (var kv in faultGates)
            {
                var lamps = kv.Value.InputLamps.OrderByDescending(l => l.Pos.Y);
                var faultLamp = lamps.First(l => l.Type == LampType.Fault);
                foreach (var lamp in lamps)
                {
                    if (lamp.Pos.Y < faultLamp.Pos.Y)
                    {
                        kv.Value.InputLamps.Remove(lamp);
                        lamp.OutputGate = null;
                        if (lamp.Type == LampType.Fault)
                        {
                            foreach (var wire in lamp.InputWires)
                            {
                                wire.Lamps.Add(faultLamp);
                                faultLamp.InputWires.Add(wire);
                            }
                        }
                        foreach (var wire in lamp.InputWires)
                        {
                            wire.Lamps.Remove(lamp);
                            lamp.InputWires.Remove(wire);
                        }
                        _lampsFound.Remove(lamp.Pos);
                    }
                }
            }
        }

        private static void PruneUnusedComponents()
        {
            bool changed = false;
            do
            {
                changed |= PruneUnusedOutputs();
                changed |= PruneUnusedGates();
                changed |= PruneUnusedLamps();
                changed |= PruneUnusedWires();
                changed |= PruneUnusedInputs();
            } while (changed);
        }

        private static bool PruneUnusedInputs()
        {
            var inputsToRemove = _inputsFound.Where(kv => kv.Value.InputPort == null || kv.Value.InputPort.OutputWires.Count == 0).ToList();
            if (inputsToRemove.Count == 0) return false;
            foreach (var kv in inputsToRemove)
            {
                kv.Value.InputPort.Inputs.Remove(kv.Value);
                kv.Value.InputPort = null;
                _inputsFound.Remove(kv.Key);
            }
            return true;
        }

        private static bool PruneUnusedOutputs()
        {
            var outputsToRemove = _outputsFound.Where(kv => kv.Value.OutputPort == null || kv.Value.OutputPort.InputWire == null).ToList();
            if (outputsToRemove.Count == 0) return false;
            foreach (var kv in outputsToRemove)
            {
                kv.Value.OutputPort.Output = null;
                kv.Value.OutputPort = null;
                _outputsFound.Remove(kv.Key);
            }
            return true;
        }

        private static bool PruneUnusedGates()
        {
            var gatesToRemove = _gatesFound.Values.Where(v => v.InputLamps.Count == 0 || v.OutputWires.Count == 0).ToList();
            if (gatesToRemove.Count == 0) return false;
            foreach (var gate in gatesToRemove)
            {
                foreach (var wire in gate.OutputWires)
                    wire.Gates.Remove(gate);
                foreach (var lamp in gate.InputLamps)
                    lamp.OutputGate = null;
                _gatesFound.Remove(gate.Pos);
            }
            return true;
        }

        private static bool PruneUnusedLamps()
        {
            var lampsToRemove = _lampsFound.Values.Where(v => v.OutputGate == null).ToList();
            if (lampsToRemove.Count == 0) return false;
            foreach (var lamp in lampsToRemove)
            {
                foreach (var wire in lamp.InputWires)
                    wire.Lamps.Remove(lamp);
                _lampsFound.Remove(lamp.Pos);
            }
            return true;
        }

        private static bool PruneUnusedWires()
        {
            var wiresToRemove = _wires.Where(v => v.OutputPorts.Count == 0 && v.Lamps.Count == 0).ToList();
            if (wiresToRemove.Count == 0) return false;
            foreach (var wire in wiresToRemove)
            {
                foreach (var gate in wire.Gates)
                    gate.OutputWires.Remove(wire);
                foreach (var inputPort in wire.InputPorts)
                    inputPort.OutputWires.Remove(wire);
                _wires.Remove(wire);
            }
            return true;
        }
    }
}
