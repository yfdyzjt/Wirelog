using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace Wirelog
{
    public static partial class Converter
    {
        private static void Postprocess()
        {
            Main.statusText = $"prune unused components";
            PruneUnusedComponents();
            Main.statusText = $"prune fault lamps";
            PruneFaultLamps();
            PruneUnusedComponents();
            Main.statusText = $"merge input ports";
            MergeInputPorts();
            Main.statusText = $"copy multi input wires and output ports";
            CopyMultiInputWiresAndOutputPorts();
            Main.statusText = $"merge output ports";
            MergeOutputPorts();
            Main.statusText = $"postprocess outputs";
            PostprocessOutput();
            PruneUnusedComponents();
            Main.statusText = $"set components id";
            SetComponentsId();
        }

        private static void PostprocessOutput()
        {
            Output.AdditionalData.Clear();
            foreach (var output in _outputsFound.Values.ToHashSet())
            {
                Output.Postprocess(output);
            }
        }

        private static void CopyMultiInputWiresAndOutputPorts()
        {
            var multiInputWires = _wires.Where(w => w.InputPorts.Count + w.Gates.Count > 1).ToHashSet();
            foreach (var curWire in multiInputWires)
            {
                var newWires = new List<Wire>();
                foreach (var gate in curWire.Gates)
                {
                    var newWire = new Wire() { Type = curWire.Type };
                    Link.Add(newWire, gate);
                    Link.Remove(curWire, gate);
                    newWires.Add(newWire);
                }
                foreach (var inputPort in curWire.InputPorts)
                {
                    var newWire = new Wire() { Type = curWire.Type };
                    Link.Add(newWire, inputPort);
                    Link.Remove(curWire, inputPort);
                    newWires.Add(newWire);
                }
                foreach (var lamp in curWire.Lamps)
                {
                    Link.Add(newWires, lamp);
                    Link.Remove(curWire, lamp);
                }
                foreach (var outputPort in curWire.OutputPorts)
                {
                    foreach (var newWire in newWires)
                    {
                        var newOutputPort = new OutputPort();
                        Link.Add(newWire, newOutputPort);
                        Link.Add(outputPort.Output, newOutputPort);
                    }
                    Link.Remove(outputPort);
                }
                _wires.AddRange(newWires);
                _wires.Remove(curWire);
            }
        }

        private static void SetComponentsId()
        {
            HashSet<InputPort> inputPorts = _inputsFound.Values.Select(input => input.InputPort).ToHashSet();
            HashSet<OutputPort> outputPorts = _outputsFound.Values.SelectMany(output => output.OutputPorts).ToHashSet();

            _inputPorts = new InputPort[inputPorts.Count];
            _outputPorts = new OutputPort[outputPorts.Count];

            int wireId = 0;
            foreach (var wire in _wires)
            {
                wire.Id = wireId++;
            }
            int inputId = 0;
            foreach (var input in _inputsFound.Values)
            {
                InputsPortFound.Add(input.Pos, input.InputPort);
            }
            foreach (var inputPort in inputPorts)
            {
                inputPort.Id = inputId++;
                _inputPorts[inputPort.Id] = inputPort;
            }
            int outputId = 0;
            foreach (var outputPort in outputPorts)
            {
                outputPort.Id = outputId++;
                _outputPorts[outputPort.Id] = outputPort;
            }
            int lampId = 0;
            foreach (var lamp in _lampsFound.Values)
            {
                lamp.Id = lampId++;
            }
            int gateId = 0;
            foreach (var gate in _gatesFound.Values)
            {
                gate.Id = gateId++;
            }
        }

        private static void PruneFaultLamps()
        {
            var faultGates = _gatesFound.Where(posGate => posGate.Value.Type == GateType.Fault);
            foreach (var posFaultGate in faultGates)
            {
                var lamps = posFaultGate.Value.Lamps.OrderByDescending(l => l.Pos.Y);
                var faultLamp = lamps.First(l => l.Type == LampType.Fault);
                foreach (var lamp in lamps)
                {
                    if (lamp.Pos.Y < faultLamp.Pos.Y)
                    {
                        if (lamp.Type == LampType.Fault)
                        {
                            Link.Add(lamp.Wires, faultLamp);
                        }
                        Link.Remove(lamp);
                        _lampsFound.Remove(lamp.Pos);
                    }
                }
            }
        }

        private static void PruneUnusedComponents()
        {
            bool changed;
            do
            {
                changed = PruneUnusedOutputs();
                changed |= PruneUnusedGates();
                changed |= PruneUnusedLamps();
                changed |= PruneUnusedWires();
                changed |= PruneUnusedInputs();
            } while (changed);
        }

        private static bool PruneUnusedInputs()
        {
            var inputsToRemove = _inputsFound.Where(posInput => posInput.Value.InputPort == null || posInput.Value.InputPort.Wires.Count == 0).ToHashSet();
            if (inputsToRemove.Count == 0) return false;
            foreach (var posInput in inputsToRemove)
            {
                Link.Remove(posInput.Value);
                _inputsFound.Remove(posInput.Key);
            }
            return true;
        }

        private static bool PruneUnusedOutputs()
        {
            var outputsToRemove = _outputsFound.Where(posOutput => posOutput.Value.OutputPorts.Count == 0 || posOutput.Value.OutputPorts.Any(outputPort => outputPort.Wire == null)).ToHashSet();
            if (outputsToRemove.Count == 0) return false;
            foreach (var posOutput in outputsToRemove)
            {
                Link.Remove(posOutput.Value);
                _outputsFound.Remove(posOutput.Key);
            }
            return true;
        }

        private static bool PruneUnusedGates()
        {
            var gatesToRemove = _gatesFound.Values.Where(gate => gate.Lamps.Count == 0 || gate.Wires.Count == 0).ToHashSet();
            if (gatesToRemove.Count == 0) return false;
            foreach (var gate in gatesToRemove)
            {
                Link.Remove(gate);
                _gatesFound.Remove(gate.Pos);
            }
            return true;
        }

        private static bool PruneUnusedLamps()
        {
            var lampsToRemove = _lampsFound.Values.Where(lamp => lamp.Gate == null).ToHashSet();
            if (lampsToRemove.Count == 0) return false;
            foreach (var lamp in lampsToRemove)
            {
                Link.Remove(lamp);
                _lampsFound.Remove(lamp.Pos);
            }
            return true;
        }

        private static bool PruneUnusedWires()
        {
            var wiresToRemove = _wires.Where(wire => wire.OutputPorts.Count == 0 && wire.Lamps.Count == 0).ToHashSet();
            if (wiresToRemove.Count == 0) return false;
            foreach (var wire in wiresToRemove)
            {
                Link.Remove(wire);
                _wires.Remove(wire);
            }
            return true;
        }

        private static void MergeOutputPorts()
        {
            // Merge all output ports that have same input. 
        }

        private static void MergeInputPorts()
        {
            var inputPortGroups = new Dictionary<string, List<InputPort>>();

            foreach (var input in _inputsFound.Values)
            {
                var key = string.Join(",", input.InputPort.Wires.Select(w => w.GetHashCode()).Order());
                var inputPorts = inputPortGroups.GetValueOrDefault(key) ?? [];
                inputPorts.Add(input.InputPort);
                inputPortGroups[key] = inputPorts;
            }

            foreach (var group in inputPortGroups.Values)
            {
                if (group.Count <= 1) continue;

                var primaryInputport = group[0];
                for (int i = 1; i < group.Count; i++)
                {
                    var inputPortToMerge = group[i];
                    foreach (var input in inputPortToMerge.Inputs)
                    {
                        Link.Remove(input, inputPortToMerge);
                        Link.Add(input, primaryInputport);
                    }
                    foreach (var wire in inputPortToMerge.Wires)
                    {
                        Link.Remove(wire, inputPortToMerge);
                        Link.Add(wire, primaryInputport);
                    }
                }
            }
        }
    }
}