using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog
{
    public class VerilogWriter
    {
        private readonly List<InputPort> _inputPorts = [];
        private readonly List<OutputPort> _outputPorts = [];
        private readonly List<Gate> _gates = [];
        private readonly List<Lamp> _lamps = [];
        private readonly List<Wire> _wires = [];

        private readonly Dictionary<Point16, Input> _inputs = [];
        private readonly Dictionary<Point16, Output> _outputs = [];
        private readonly Dictionary<Point16, Gate> _gatesFound = [];
        private readonly Dictionary<Point16, Lamp> _lampsFound = [];

        private readonly HashSet<Point16> _processedTiles = [];

        public string GenerateVerilog()
        {
            Preprocess();
            Prune();
            return ConvertToVerilog();
        }

        private void Preprocess()
        {
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    var pos = new Point16(x, y);
                    if (_processedTiles.Contains(pos)) continue;

                    Tile tile = Main.tile[x, y];
                    if (tile == null || !tile.HasTile) continue;

                    if (Input.TryGetType(tile, out var inputType))
                    {
                        bool hasWire = false;
                        var (sizeX, sizeY) = Input.GetSize(inputType);
                        for (int i = 0; i < sizeX; i++)
                        {
                            for (int j = 0; j < sizeY; j++)
                            {
                                var curPos = new Point16(x + i, y + j);
                                _processedTiles.Add(curPos);
                                if (HasWire(curPos)) hasWire = true;
                            }
                        }
                        if (hasWire) _inputs.Add(pos, new Input { Id = _inputs.Count, Type = inputType });
                    }
                    else if (Output.TryGetType(tile, out var outputType))
                    {
                        bool hasWire = false;
                        var (sizeX, sizeY) = Output.GetSize(outputType);
                        for (int i = 0; i < sizeX; i++)
                        {
                            for (int j = 0; j < sizeY; j++)
                            {
                                var curPos = new Point16(x + i, y + j);
                                _processedTiles.Add(curPos);
                                if (HasWire(curPos)) hasWire = true;
                            }
                        }
                        if (hasWire) _outputs.Add(pos, new Output { Id = _outputs.Count, Type = outputType });
                    }
                    else if (Gate.TryGetType(tile, out var gateType))
                    {
                        _gatesFound.Add(pos, new Gate { Id = _gatesFound.Count, Type = gateType });
                        _processedTiles.Add(pos);
                    }
                    else if (Lamp.TryGetType(tile, out var lampType))
                    {
                        _lampsFound.Add(pos, new Lamp { Id = _lampsFound.Count, Type = lampType });
                        _processedTiles.Add(pos);
                    }
                }
            }

            ConnectComponents();
        }

        private void ConnectComponents()
        {
            var visitedWires = new HashSet<(Point16, int)>();

            foreach (var inputEntry in _inputs)
            {
                var inputPos = inputEntry.Key;
                var input = inputEntry.Value;

                var (sizeX, sizeY) = Input.GetSize(input.Type);

                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        for (int color = 0; color < 4; color++)
                        {
                            var curPos = new Point16(inputPos.X + i, inputPos.Y + j);
                            if (HasWire(curPos, color) && !visitedWires.Contains((curPos, color)))
                            {
                                var wire = new Wire { Id = _wires.Count };
                                var port = new InputPort { Id = _inputPorts.Count };
                                _inputPorts.Add(port);
                                _wires.Add(wire);
                            }
                        }
                    }
                }

                for (int color = 0; color < 4; color++)
                {
                    if (HasWire(inputPos, color) && !visitedWires.Contains((inputPos, color)))
                    {
                        var wire = new Wire { Id = _wires.Count };
                        var inputPort = GetOrCreateInputPort(input);
                        wire.InputPortIDs.Add(inputPort.Id);
                        inputPort.OutputWireIDs.Add(wire.Id);

                        TraceWire(inputPos, color, wire, visitedWires);
                        _wires.Add(wire);
                    }
                }
            }
        }

        private void TraceWire(Point16 startPos, int color, Wire wire, HashSet<(Point16, int)> visitedWires)
        {
            var queue = new Queue<Point16>();
            queue.Enqueue(startPos);
            visitedWires.Add((startPos, color));

            while (queue.Count > 0)
            {
                var currentPos = queue.Dequeue();

                // Check for components at the current position
                if (_lampsFound.TryGetValue(currentPos, out var lamp))
                {
                    if (!wire.LampIDs.Contains(lamp.Id))
                    {
                        wire.LampIDs.Add(lamp.Id);
                    }
                }
                if (_gatesFound.TryGetValue(currentPos, out var gate))
                {
                    if (!wire.GateIDs.Contains(gate.Id))
                    {
                        wire.GateIDs.Add(gate.Id);
                    }
                }
                if (_outputs.TryGetValue(currentPos, out var output))
                {
                    var outputPort = GetOrCreateOutputPort(output);
                    if (!wire.OutputPortIDs.Contains(outputPort.Id))
                    {
                        wire.OutputPortIDs.Add(outputPort.Id);
                        outputPort.InputWireID = wire.Id;
                    }
                }

                // Explore adjacent wires
                foreach (var neighbor in GetNeighbors(currentPos))
                {
                    if (HasWire(neighbor, color) && !visitedWires.Contains((neighbor, color)))
                    {
                        visitedWires.Add((neighbor, color));
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        private static bool HasWire(Point16 pos)
        {
            return HasWire(pos, 0) || HasWire(pos, 1) || HasWire(pos, 2) || HasWire(pos, 3);
        }

        private static bool HasWire(Point16 pos, int color)
        {
            var tile = Main.tile[pos.X, pos.Y];
            return color switch
            {
                0 => tile.RedWire,
                1 => tile.GreenWire,
                2 => tile.BlueWire,
                3 => tile.YellowWire,
                _ => false,
            };
        }

        private IEnumerable<Point16> GetNeighbors(Point16 pos)
        {
            if (pos.X > 0) yield return new Point16(pos.X - 1, pos.Y);
            if (pos.X < Main.maxTilesX - 1) yield return new Point16(pos.X + 1, pos.Y);
            if (pos.Y > 0) yield return new Point16(pos.X, pos.Y - 1);
            if (pos.Y < Main.maxTilesY - 1) yield return new Point16(pos.X, pos.Y + 1);
        }

        private InputPort GetOrCreateInputPort(Input input)
        {
            // Simple case: one port per input. Pruning will merge them.
            var port = new InputPort { Id = _inputPorts.Count };
            _inputPorts.Add(port);
            return port;
        }

        private OutputPort GetOrCreateOutputPort(Output output)
        {
            // Simple case: one port per output.
            var port = new OutputPort { Id = _outputPorts.Count };
            _outputPorts.Add(port);
            return port;
        }

        private void Prune()
        {
            bool changed;
            do
            {
                changed = PruneUnusedWires();
                changed |= PruneUnusedGatesAndLamps();
            } while (changed);

            MergeInputPorts();
        }

        private bool PruneUnusedWires()
        {
            var changed = false;
            var wiresToRemove = new List<Wire>();

            foreach (var wire in _wires)
            {
                if (wire.OutputPortIDs.Count == 0 && wire.GateIDs.Count == 0) // Simplified check
                {
                    wiresToRemove.Add(wire);
                    changed = true;
                }
            }

            foreach (var wire in wiresToRemove)
            {
                _wires.Remove(wire);
                // Also remove references to this wire from input ports
                foreach (var inputPort in _inputPorts)
                {
                    inputPort.OutputWireIDs.Remove(wire.Id);
                }
            }

            return changed;
        }

        private bool PruneUnusedGatesAndLamps()
        {
            var changed = false;
            var gatesToRemove = new HashSet<int>();
            var lampsToRemove = new HashSet<int>();

            // First, mark all gates and lamps that don't lead to outputs
            foreach (var wire in _wires)
            {
                if (wire.OutputPortIDs.Count == 0)
                {
                    foreach (var gateId in wire.GateIDs)
                    {
                        gatesToRemove.Add(gateId);
                    }
                    foreach (var lampId in wire.LampIDs)
                    {
                        lampsToRemove.Add(lampId);
                    }
                    changed = true;
                }
            }

            // Remove marked gates and lamps from wires
            foreach (var wire in _wires)
            {
                wire.GateIDs.RemoveAll(id => gatesToRemove.Contains(id));
                wire.LampIDs.RemoveAll(id => lampsToRemove.Contains(id));
            }

            return changed;
        }

        private void MergeInputPorts()
        {
            var portGroups = new Dictionary<string, List<InputPort>>();

            // Group ports by their output wire patterns
            foreach (var port in _inputPorts)
            {
                var key = GetPortOutputPattern(port);
                if (!portGroups.ContainsKey(key))
                {
                    portGroups[key] = new List<InputPort>();
                }
                portGroups[key].Add(port);
            }

            // Merge ports in each group
            foreach (var group in portGroups.Values)
            {
                if (group.Count <= 1) continue;

                var primaryPort = group[0];
                for (int i = 1; i < group.Count; i++)
                {
                    var portToMerge = group[i];
                    _inputPorts.Remove(portToMerge);
                }
            }
        }

        private string GetPortOutputPattern(InputPort port)
        {
            // Create a unique string representation of the port's output connections
            var wireIds = port.OutputWireIDs.OrderBy(id => id);
            return string.Join(",", wireIds);
        }

        private List<int> GetInputWiresForLamp(Lamp lamp)
        {
            return _wires.Where(w => w.LampIDs.Contains(lamp.Id)).Select(w => w.Id).ToList();
        }

        private List<int> GetInputLampsForGate(Gate gate)
        {
            return _lamps.Where(l => l.GateId == gate.Id).Select(l => l.Id).ToList();
        }

        private int GetOutputWireForGate(Gate gate)
        {
            var wire = _wires.FirstOrDefault(w => w.GateIDs.Contains(gate.Id));
            return wire?.Id ?? -1;
        }

        private string GetLampModuleName(Lamp lamp, int inputCount)
        {
            var typeStr = lamp.Type switch
            {
                LampType.On => "On",
                LampType.Off => "Off",
                LampType.Fault => "Fault",
                _ => throw new ArgumentException($"Unknown lamp type: {lamp.Type}")
            };
            return $"Lamp_{(inputCount == 1 ? "Single" : "Multi")}_{typeStr}";
        }

        private string GetGateModuleName(Gate gate, int inputCount)
        {
            var typeStr = gate.Type switch
            {
                GateType.AND => "AND",
                GateType.OR => "OR",
                GateType.NAND => "NAND",
                GateType.NOR => "NOR",
                GateType.XOR => "XOR",
                GateType.XNOR => "XNOR",
                GateType.Fault => "Fault",
                _ => throw new ArgumentException($"Unknown gate type: {gate.Type}")
            };
            return $"Gate_{(inputCount == 1 ? "Single" : "Multi")}_{typeStr}";
        }

        private string ConvertToVerilog()
        {
            var sb = new StringBuilder();

            sb.AppendLine("`default_nettype none");
            sb.AppendLine("module Wiring(");
            sb.AppendLine("    input wire clk,");
            sb.AppendLine($"    input wire[{_inputPorts.Count - 1}:0] in,");
            sb.AppendLine($"    output wire[{_outputPorts.Count - 1}:0] out");
            sb.AppendLine(");");

            sb.AppendLine($"    wire[{_wires.Count - 1}:0] wires;");
            sb.AppendLine($"    wire[{_lamps.Count - 1}:0] lamps;");

            // Instantiate Input Ports
            foreach (var port in _inputPorts)
            {
                sb.AppendLine($"    assign wires[{string.Join(" | wires[", port.OutputWireIDs)}] = in[{port.Id}];");
            }

            // Instantiate Lamps
            foreach (var lamp in _lamps)
            {
                var inputWires = GetInputWiresForLamp(lamp);
                if (inputWires.Count == 0) continue;

                var moduleName = GetLampModuleName(lamp, inputWires.Count);
                sb.AppendLine($"    {moduleName} lamp_{lamp.Id} (");
                sb.AppendLine($"        .clk(clk),");
                for (int i = 0; i < inputWires.Count; i++)
                {
                    sb.AppendLine($"        .in{i}(wires[{inputWires[i]}]),");
                }
                sb.AppendLine($"        .out(lamps[{lamp.Id}])");
                sb.AppendLine("    );");
            }

            // Instantiate Gates
            foreach (var gate in _gates)
            {
                var inputLamps = GetInputLampsForGate(gate);
                if (inputLamps.Count == 0) continue;

                var outputWire = GetOutputWireForGate(gate);
                if (outputWire == -1) continue;

                var moduleName = GetGateModuleName(gate, inputLamps.Count);
                sb.AppendLine($"    {moduleName} gate_{gate.Id} (");
                sb.AppendLine($"        .clk(clk),");
                for (int i = 0; i < inputLamps.Count; i++)
                {
                    sb.AppendLine($"        .in{i}(lamps[{inputLamps[i]}]),");
                }
                sb.AppendLine($"        .out(wires[{outputWire}])");
                sb.AppendLine("    );");
            }

            // Instantiate Output Ports
            foreach (var port in _outputPorts)
            {
                sb.AppendLine($"    assign out[{port.Id}] = wires[{port.InputWireID}];");
            }

            sb.AppendLine("endmodule");

            return sb.ToString();
        }
    }
}