using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog
{
    public static partial class Converter
    {
        public static void PublicTraceWire(
            Wire wire,
            Point16 curPos,
            Point16 prevPos,
            int level,
            HashSet<(Point16, WireType)> visitedWires,
            Action<Wire, Point16, int> traceComponents
            ) => TraceWire(wire, curPos, prevPos, level, visitedWires, traceComponents);

        private static void Preprocess()
        {
            PreprocessComponents();
            PreprocessWires();
        }

        private static void PreprocessComponents()
        {
            HashSet<Point16> visitedTiles = [];

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (x % Math.Max(1, Main.maxTilesX / 100) == 0)
                    Main.statusText = $"preprocess components {x * 1f / Main.maxTilesX:P1}";
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    var pos = new Point16(x, y);
                    if (visitedTiles.Contains(pos)) continue;

                    Tile tile = Main.tile[pos];
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
                                Input.TryGetType(Main.tile[curPos], out var curInputType);
                                if (curInputType == inputType)
                                {
                                    visitedTiles.Add(curPos);
                                    if (Wire.HasWire(curPos)) hasWire = true;
                                }
                            }
                        }
                        if (hasWire)
                        {
                            var input = new Input { Type = inputType, Pos = pos };
                            for (int i = 0; i < sizeX; i++)
                            {
                                for (int j = 0; j < sizeY; j++)
                                {
                                    var curPos = new Point16(x + i, y + j);
                                    Input.TryGetType(Main.tile[curPos], out var curInputType);
                                    if (Wire.HasWire(curPos) && curInputType == inputType)
                                    {
                                        _inputsFound.Add(curPos, input);
                                    }
                                }
                            }
                        }
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
                                Output.TryGetType(Main.tile[curPos], out var curOutputType);
                                if (curOutputType == outputType)
                                {
                                    visitedTiles.Add(curPos);
                                    if (Wire.HasWire(curPos)) hasWire = true;
                                }
                            }
                        }
                        if (hasWire)
                        {
                            var output = new Output { Type = outputType, Pos = pos };
                            for (int i = 0; i < sizeX; i++)
                            {
                                for (int j = 0; j < sizeY; j++)
                                {
                                    var curPos = new Point16(x + i, y + j);
                                    Output.TryGetType(Main.tile[curPos], out var curOutputType);
                                    if (Wire.HasWire(curPos) && curOutputType == outputType)
                                    {
                                        _outputsFound.Add(curPos, output);
                                    }
                                }
                            }
                        }
                    }
                    else if (Gate.TryGetType(tile, out var gateType))
                    {
                        visitedTiles.Add(pos);
                        _gatesFound.Add(pos, new Gate { Type = gateType, Pos = pos });
                    }
                    else if (Lamp.TryGetType(tile, out var lampType))
                    {
                        visitedTiles.Add(pos);
                        _lampsFound.Add(pos, new Lamp { Type = lampType, Pos = pos });
                    }
                }
            }
        }

        private static void PreprocessWires()
        {
            var visitedWires = new HashSet<(Point16, WireType)>();

            int i = 0;
            int sourceCount = _inputsFound.Count + _gatesFound.Count;
            foreach (var posInput in _inputsFound)
            {
                if (i % Math.Max(1, sourceCount / 100) == 0)
                    Main.statusText = $"connect components {i++ * 1f / sourceCount:P1}";
                TraceSource(posInput.Key, visitedWires);
            }
            foreach (var posGate in _gatesFound)
            {
                if (i % Math.Max(1, sourceCount / 100) == 0)
                    Main.statusText = $"connect components {i++ * 1f / sourceCount:P1}";
                TraceSource(posGate.Key, visitedWires);
            }
        }

        private static void TraceSource(Point16 startPos, HashSet<(Point16, WireType)> visitedWires)
        {
            foreach (WireType wireType in Enum.GetValues(typeof(WireType)))
            {
                if (Wire.HasWire(startPos, wireType) && !visitedWires.Contains((startPos, wireType)))
                {
                    var wire = new Wire() { Type = wireType };
                    TraceWire(wire, startPos, startPos, 0, visitedWires, TraceComponents);
                    _wires.Add(wire);
                }
            }
        }

        private static void TraceWire(
            Wire wire,
            Point16 curPos,
            Point16 prevPos,
            int level,
            HashSet<(Point16, WireType)> visitedWires,
            Action<Wire, Point16, int> traceComponents
            )
        {
            if (!WorldGen.InWorld(curPos.X, curPos.Y, 1)) return;

            Tile tile = Main.tile[curPos];

            if (!Wire.HasWire(tile, wire.Type)) return;
            if (!JunctionBox.TryGetType(tile, out _) && visitedWires.Contains((curPos, wire.Type))) return;

            visitedWires.Add((curPos, wire.Type));
            traceComponents(wire, curPos, level);

            if (JunctionBox.TryGetType(tile, out var junctionBoxType))
            {
                int dX = 0, dY = 0;
                switch (junctionBoxType)
                {
                    case JunctionBoxType.UpDown:
                        dX = (curPos.X - prevPos.X);
                        dY = (curPos.Y - prevPos.Y);
                        break;
                    case JunctionBoxType.UpLeft:
                        dX = -(curPos.Y - prevPos.Y);
                        dY = -(curPos.X - prevPos.X);
                        break;
                    case JunctionBoxType.UpRight:
                        dX = (curPos.Y - prevPos.Y);
                        dY = (curPos.X - prevPos.X);
                        break;
                }
                var newPos = new Point16(curPos.X + dX, curPos.Y + dY);
                TraceWire(wire, newPos, curPos, level + 1, visitedWires, traceComponents);
            }
            else
            {
                bool prevJunction = JunctionBox.TryGetType(Main.tile[prevPos], out _);
                foreach (var (dX, dY) in new (int, int)[] { (1, 0), (0, 1), (-1, 0), (0, -1) })
                {
                    var newPos = new Point16(curPos.X + dX, curPos.Y + dY);
                    if (!(prevJunction && prevPos == newPos))
                    {
                        TraceWire(wire, newPos, curPos, level + 1, visitedWires, traceComponents);
                    }
                }
            }
        }

        private static void TraceComponents(Wire wire, Point16 curPos, int level)
        {
            if (_lampsFound.TryGetValue(curPos, out var foundLamp))
            {
                if (TraceGate(foundLamp, out var foundGate))
                {
                    foundLamp.InputWires.Add(wire);
                    wire.Lamps.Add(foundLamp);
                }
                else
                {
                    _lampsFound.Remove(curPos);
                }
            }
            else if (_gatesFound.TryGetValue(curPos, out var foundGate))
            {
                if (TraceLamp(foundGate, out var foundLamps))
                {
                    foundGate.OutputWires.Add(wire);
                    wire.Gates.Add(foundGate);

                    foreach (var lamp in foundLamps)
                    {
                        lamp.OutputGate = foundGate;
                        foundGate.InputLamps.Add(lamp);
                    }
                }
                else
                {
                    _gatesFound.Remove(curPos);
                }
            }
            else if (_inputsFound.TryGetValue(curPos, out var foundInput))
            {
                if (wire.InputPorts.All(inputPort => inputPort.Inputs.All(input => input.Pos != foundInput.Pos)))
                {
                    foundInput.InputPort ??= new InputPort();
                    foundInput.InputPort.Inputs.Add(foundInput);
                    foundInput.InputPort.OutputWires.Add(wire);
                    wire.InputPorts.Add(foundInput.InputPort);
                }
            }
            else if (_outputsFound.TryGetValue(curPos, out var foundOutput))
            {
                if (wire.OutputPorts.All(outputPort => outputPort.Output.Pos != foundOutput.Pos))
                {
                    var outputPort = new OutputPort();
                    foundOutput.OutputPorts.Add(outputPort);
                    outputPort.Output = foundOutput;
                    wire.OutputPorts.Add(outputPort);
                    outputPort.InputWire = wire;
                }
            }
        }

        private static bool TraceGate(Lamp lamp, out Gate outGate)
        {
            if (lamp.OutputGate == null)
            {
                for (int y = 1; ; y++)
                {
                    var curPos = new Point16(lamp.Pos.X, lamp.Pos.Y + y);
                    if (_gatesFound.TryGetValue(curPos, out var gate))
                    {
                        outGate = gate;
                        return true;
                    }
                    else if (!_lampsFound.ContainsKey(curPos))
                    {
                        outGate = null;
                        return false;
                    }
                }
            }
            else
            {
                outGate = lamp.OutputGate;
                return true;
            }
        }

        private static bool TraceLamp(Gate gate, out List<Lamp> outLamps)
        {
            outLamps = [];
            for (int y = -1; ; y--)
            {
                var curPos = new Point16(gate.Pos.X, gate.Pos.Y + y);
                if (_lampsFound.TryGetValue(curPos, out var lamp))
                {
                    outLamps.Add(lamp);
                }
                else
                {
                    break;
                }
            }
            return outLamps.Count != 0;
        }
    }
}