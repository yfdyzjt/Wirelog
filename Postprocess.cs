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
            Main.statusText = $"set components id";
            SetComponentsId();
        }

        private static void CopyMultiInputWiresAndOutputPorts()
        {
            var multiInputWires = _wires.Where(w => w.InputPorts.Count + w.Gates.Count > 1).ToList();
            foreach (var curWire in multiInputWires)
            {
                var newWires = new List<Wire>();
                foreach (var gate in curWire.Gates)
                {
                    var newWire = new Wire() { Type = curWire.Type };
                    gate.OutputWires.Add(newWire);
                    newWire.Gates.Add(gate);
                    gate.OutputWires.Remove(curWire);
                    curWire.Gates.Remove(gate);
                    newWires.Add(newWire);
                }
                foreach (var inputPort in curWire.InputPorts)
                {
                    var newWire = new Wire() { Type = curWire.Type };
                    newWire.InputPorts.Add(inputPort);
                    inputPort.OutputWires.Add(newWire);
                    curWire.InputPorts.Remove(inputPort);
                    inputPort.OutputWires.Remove(curWire);
                    newWires.Add(newWire);
                }
                foreach (var lamp in curWire.Lamps)
                {
                    foreach (var newWire in newWires)
                    {
                        newWire.Lamps.Add(lamp);
                        lamp.InputWires.Add(newWire);
                    }
                    curWire.Lamps.Remove(lamp);
                    lamp.InputWires.Remove(curWire);
                }
                foreach (var outputPort in curWire.OutputPorts)
                {
                    foreach (var newWire in newWires)
                    {
                        var newOutputPort = new OutputPort
                        {
                            InputWire = newWire,
                            Output = outputPort.Output
                        };
                        newWire.OutputPorts.Add(newOutputPort);
                        outputPort.Output.OutputPorts.Add(newOutputPort);
                    }
                    curWire.OutputPorts.Remove(outputPort);
                    outputPort.Output.OutputPorts.Remove(outputPort);
                    outputPort.InputWire = null;
                    outputPort.Output = null;
                }
                _wires.AddRange(newWires);
                _wires.Remove(curWire);
            }
        }

        private static void SetComponentsId()
        {
            HashSet<InputPort> inputPorts = _inputsFound.Values.Select(input => input.InputPort).ToHashSet();
            HashSet<OutputPort> outputPorts = _outputsFound.Values.SelectMany(output => output.OutputPorts).ToHashSet();

            int wireId = 0;
            foreach (var wire in _wires)
            {
                wire.Id = wireId++;
            }
            int inputId = 0;
            foreach (var inputPort in inputPorts)
            {
                inputPort.Id = inputId++;
                _inputsPortFound.Add(inputPort.Id, inputPort);
            }
            int outputId = 0;
            foreach (var outputPort in outputPorts)
            {
                outputPort.Id = outputId++;
                _outputsPortFound.Add(outputPort.Id, outputPort);
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
                var lamps = posFaultGate.Value.InputLamps.OrderByDescending(l => l.Pos.Y);
                var faultLamp = lamps.First(l => l.Type == LampType.Fault);
                foreach (var lamp in lamps)
                {
                    if (lamp.Pos.Y < faultLamp.Pos.Y)
                    {
                        posFaultGate.Value.InputLamps.Remove(lamp);
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
            var inputsToRemove = _inputsFound.Where(posInput => posInput.Value.InputPort == null || posInput.Value.InputPort.OutputWires.Count == 0).ToList();
            if (inputsToRemove.Count == 0) return false;
            foreach (var posInput in inputsToRemove)
            {
                posInput.Value.InputPort.Inputs.Remove(posInput.Value);
                posInput.Value.InputPort = null;
                _inputsFound.Remove(posInput.Key);
            }
            return true;
        }

        private static bool PruneUnusedOutputs()
        {
            var outputsToRemove = _outputsFound.Where(posOutput => posOutput.Value.OutputPorts.Count == 0 || posOutput.Value.OutputPorts.Any(outputPort => outputPort.InputWire == null)).ToList();
            if (outputsToRemove.Count == 0) return false;
            foreach (var posOutput in outputsToRemove)
            {
                foreach (var outputPort in posOutput.Value.OutputPorts)
                {
                    outputPort.Output.OutputPorts.Remove(outputPort);
                }
                posOutput.Value.OutputPorts.Clear();
                _outputsFound.Remove(posOutput.Key);
            }
            return true;
        }

        private static bool PruneUnusedGates()
        {
            var gatesToRemove = _gatesFound.Values.Where(gate => gate.InputLamps.Count == 0 || gate.OutputWires.Count == 0).ToList();
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
            var lampsToRemove = _lampsFound.Values.Where(lamp => lamp.OutputGate == null).ToList();
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
            var wiresToRemove = _wires.Where(wire => wire.OutputPorts.Count == 0 && wire.Lamps.Count == 0).ToList();
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

        private static void MergeInputPorts()
        {
            var inputPortGroups = new Dictionary<string, List<InputPort>>();

            foreach (var input in _inputsFound.Values)
            {
                var key = string.Join(",", input.InputPort.OutputWires.Select(w => w.GetHashCode()).Order());
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
                        primaryInputport.Inputs.Add(input);
                        inputPortToMerge.Inputs.Remove(input);
                        input.InputPort = primaryInputport;
                    }
                    foreach (var wire in inputPortToMerge.OutputWires)
                    {
                        primaryInputport.OutputWires.Add(wire);
                        inputPortToMerge.OutputWires.Remove(wire);
                        wire.InputPorts.Add(primaryInputport);
                        wire.InputPorts.Remove(inputPortToMerge);
                    }
                }
            }
        }
    }
}