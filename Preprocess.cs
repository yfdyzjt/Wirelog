using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog
{
    public partial class Converter
    {
        private static void Preprocess()
        {
            _inputsFound.Clear();
            _outputsFound.Clear();
            _gatesFound.Clear();
            _lampsFound.Clear();
            _wires.Clear();

            HashSet<Point16> _processedTiles = [];

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    var pos = new Point16(x, y);
                    if (_processedTiles.Contains(pos)) continue;

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
                                _processedTiles.Add(curPos);
                                if (Wire.HasWire(Main.tile[curPos])) hasWire = true;
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
                                    if (Wire.HasWire(Main.tile[curPos]))
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
                                _processedTiles.Add(curPos);
                                if (Wire.HasWire(Main.tile[curPos])) hasWire = true;
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
                                    if (Wire.HasWire(Main.tile[curPos]))
                                    {
                                        _outputsFound.Add(curPos, output);
                                    }
                                }
                            }
                        }
                    }
                    else if (Gate.TryGetType(tile, out var gateType))
                    {
                        _processedTiles.Add(pos);
                        _gatesFound.Add(pos, new Gate { Type = gateType, Pos = pos });
                    }
                    else if (Lamp.TryGetType(tile, out var lampType))
                    {
                        _processedTiles.Add(pos);
                        _lampsFound.Add(pos, new Lamp { Type = lampType, Pos = pos });
                    }
                }
            }

            ConnectComponents();
        }

        private static void ConnectComponents()
        {
            var visitedWires = new HashSet<(Point16, WireType)>();
            foreach (var inputEntry in _inputsFound)
            {
                TraceSource(inputEntry.Key, visitedWires);
            }
        }

        private static void TraceSource(Point16 startPos, HashSet<(Point16, WireType)> visitedWires)
        {
            foreach (WireType wireType in Enum.GetValues(typeof(WireType)))
            {
                if (Wire.HasWire(Main.tile[startPos], wireType))
                {
                    var wire = new Wire();
                    _wires.Add(wire);
                    TraceWire(wire, startPos, startPos, wireType, 0, visitedWires);
                }
            }
        }

        private static void TraceWire(Wire wire, Point16 curPos, Point16 prevPos, WireType wireType, int level, HashSet<(Point16, WireType)> visitedWires)
        {
            if (!WorldGen.InWorld(curPos.X, curPos.Y, 1)) return;

            Tile tile = Main.tile[curPos];

            if (!JunctionBox.TryGetType(tile, out _) && visitedWires.Contains((curPos, wireType))) return;

            visitedWires.Add((curPos, wireType));
            TraceComponents(wire, curPos, level, visitedWires);

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
                TraceWire(wire, newPos, curPos, wireType, level + 1, visitedWires);
            }
            else
            {
                bool prevJunction = JunctionBox.TryGetType(Main.tile[prevPos], out _);
                foreach (var (dX, dY) in new (int, int)[] { (1, 0), (0, 1), (-1, 0), (0, -1) })
                {
                    var newPos = new Point16(curPos.X + dX, curPos.Y + dY);
                    if (!(prevJunction && prevPos == newPos))
                    {
                        TraceWire(wire, newPos, curPos, wireType, level + 1, visitedWires);
                    }
                }
            }
        }

        private static void TraceComponents(Wire wire, Point16 curPos, int level, HashSet<(Point16, WireType)> visitedWires)
        {
            if (_lampsFound.TryGetValue(curPos, out var foundLamp))
            {
                if (TraceGate(foundLamp, curPos, out var foundGate))
                {
                    foundLamp.InputWires.Add(wire);
                    wire.Lamps.Add(foundLamp);

                    foundLamp.OutputGate = foundGate;
                    foundGate.InputLamps.Add(foundLamp);

                    TraceSource(foundGate.Pos, visitedWires);
                }
                else
                {
                    _lampsFound.Remove(curPos);
                }
            }
            else if (_gatesFound.TryGetValue(curPos, out var foundGate))
            {
                if (TraceLamp(foundGate, curPos))
                {
                    foundGate.OutputWires.Add(wire);
                    wire.Gates.Add(foundGate);
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
                    foundOutput.OutputPort ??= new OutputPort();
                    foundOutput.OutputPort.Output = foundOutput;
                    foundOutput.OutputPort.InputWire = wire;
                    wire.OutputPorts.Add(foundOutput.OutputPort);
                }
            }
        }

        private static bool TraceGate(Lamp lamp, Point16 lampPos, out Gate outGate)
        {
            if (lamp.OutputGate == null)
            {
                for (int y = 1; ; y++)
                {
                    var curPos = new Point16(lampPos.X, lampPos.Y + y);
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

        private static bool TraceLamp(Gate gate, Point16 gatePos)
        {
            if (gate.InputLamps.Count == 0)
            {
                return _lampsFound.ContainsKey(new Point16(gatePos.X, gatePos.Y - 1));
            }
            else
            {
                return true;
            }
        }
    }
}