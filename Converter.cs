using Ionic.BZip2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static log4net.Appender.ColoredConsoleAppender;

namespace Wirelog
{
    public class Converter
    {
        public static readonly Dictionary<Point16, int> posInputIdMap = [];
        public static readonly Dictionary<int, Point16> posOutputIdMap = [];

        private static readonly List<InputPort> _inputPorts = [];
        private static readonly List<OutputPort> _outputPorts = [];
        private static readonly List<Gate> _gates = [];
        private static readonly List<Lamp> _lamps = [];
        private static readonly List<Wire> _wires = [];

        private static readonly Dictionary<Point16, Input> _inputsFound = [];
        private static readonly Dictionary<Point16, Output> _outputsFound = [];
        private static readonly Dictionary<Point16, Gate> _gatesFound = [];
        private static readonly Dictionary<Point16, Lamp> _lampsFound = [];

        private static readonly HashSet<Point16> _processedTiles = [];

        private static void Preprocess()
        {
            _inputsFound.Clear();
            _outputsFound.Clear();
            _gatesFound.Clear();
            _lampsFound.Clear();

            _processedTiles.Clear();

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
                                        _inputsFound.Add(curPos, input);
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
                                        _outputsFound.Add(curPos, output);
                                }
                            }
                        }
                    }
                    else if (Gate.TryGetType(tile, out var gateType))
                    {
                        _processedTiles.Add(pos);
                        _gatesFound.Add(pos, new Gate { Type = gateType });
                    }
                    else if (Lamp.TryGetType(tile, out var lampType))
                    {
                        _processedTiles.Add(pos);
                        _lampsFound.Add(pos, new Lamp { Type = lampType });
                    }
                }
            }

            ConnectComponents();
        }

        private static void ConnectComponents()
        {
            var visitedWires = new HashSet<(Point16, WireType)>();

            foreach (var inputEntry in _inputsFound.ToList())
            {
                if (!_inputsFound.ContainsKey(inputEntry.Key)) continue;

                var inputPos = inputEntry.Key;
                var input = inputEntry.Value;

                TraceSource(inputPos, input, visitedWires);
            }
        }

        private static void TraceSource(Point16 startPos, Input input, HashSet<(Point16, WireType)> visitedWires)
        {
            foreach (WireType wireType in Enum.GetValues(typeof(WireType)))
            {
                if (Wire.HasWire(Main.tile[startPos], wireType))
                {
                    var wire = new Wire() { };
                    TraceWire(wire, startPos, startPos, wireType, 0, visitedWires);
                }
            }
        }

        private static void TraceSink(Wire wire, Point16 curPos, int level)
        {
            if(_lampsFound.TryGetValue(curPos, out var lamp))
            {
                lamp.InputWires.Add(wire);
                wire.Lamps.Add(lamp);
            }
            else if(_gatesFound.TryGetValue(curPos, out var gate))
            {
                gate.OutputWires.Add(wire);
                wire.Gates.Add(gate);
            }
            else if(_inputsFound.TryGetValue(curPos, out var input))
            {
            }
            else if(_outputsFound.TryGetValue(curPos, out var output))
            {
            }
        }

        private static void TraceWire(Wire wire, Point16 curPos, Point16 prevPos, WireType wireType, int level, HashSet<(Point16, WireType)> visitedWires)
        {
            if (!WorldGen.InWorld(curPos.X, curPos.Y, 1)) return;

            Tile tile = Main.tile[curPos];

            if (!JunctionBox.TryGetType(tile, out _) && visitedWires.Contains((curPos, wireType))) return;

            visitedWires.Add((curPos, wireType));
            TraceSink(wire, curPos, level);

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
    }
}