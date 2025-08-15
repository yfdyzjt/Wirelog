using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Teleporter
    {
        public static void Activate(OutputPort outputPort)
        {
            var curPos = outputPort.Output.Pos;
            if (Output.AdditionalData.TryGetValue(curPos, out var obj))
            {
                var teleporterMap = (Dictionary<OutputPort, Point16>)obj;
                if (teleporterMap.TryGetValue(outputPort, out var nextPos))
                {
                    WiringWrapper.TeleportPos[0].X = curPos.X;
                    WiringWrapper.TeleportPos[0].Y = curPos.Y;
                    WiringWrapper.TeleportPos[1].X = nextPos.X;
                    WiringWrapper.TeleportPos[1].Y = nextPos.Y;
                    WiringWrapper.Teleport();
                }
            }
        }

        public static void Postprocess(Output output)
        {
            var teleporterMap = new Dictionary<OutputPort, Point16>();

            var inputLinks = new Dictionary<(Point16, WireType), OutputPort>();
            var visitedWires = new HashSet<(Point16, WireType)>();
            var (sizeX, sizeY) = Output.GetSize(output.Type);

            for (var x = 0; x < sizeX; x++)
            {
                for (var y = 0; y < sizeY; y++)
                {
                    var startPos = new Point16(output.Pos.X + x, output.Pos.Y + y);
                    foreach (WireType wireType in Enum.GetValues(typeof(WireType)))
                    {
                        if (!Wire.HasWire(startPos, wireType)) continue;

                        var wire = new Wire() { Type = wireType };
                        Converter.PublicTraceWire(wire, startPos, startPos, 0, visitedWires,
                            (wire, pos, level) =>
                            {
                                OutputPort outputPort = null;
                                if (Converter.InputsFound.TryGetValue(pos, out var input))
                                {
                                    outputPort = output.OutputPorts
                                    .Where(o => o.Wire.InputPorts
                                    .FirstOrDefault()?.Inputs
                                    .Any(i => i == input) == true)
                                    .FirstOrDefault();
                                }
                                else if (Converter.GatesFound.TryGetValue(pos, out var gate))
                                {
                                    outputPort = output.OutputPorts
                                    .Where(o => o.Wire.Gates
                                    .FirstOrDefault() == gate)
                                    .FirstOrDefault();
                                }
                                if (outputPort != null)
                                {
                                    inputLinks.Add((pos, wire.Type), outputPort);
                                }
                            });
                    }
                }
            }

            foreach (var inputLink in inputLinks)
            {
                var outputPort = inputLink.Value;
                var startPos = inputLink.Key.Item1;
                var wireType = inputLink.Key.Item2;
                int minLevel = int.MaxValue;
                int maxLevel = 0;
                Output minOutput = null;
                Output maxOutput = null;
                var wire = new Wire() { Type = wireType };
                visitedWires.Clear();
                Converter.PublicTraceWire(wire, startPos, startPos, 0, visitedWires,
                            (wire, pos, level) =>
                            {
                                if (!Converter.OutputsFound.TryGetValue(pos, out var foundOutput)) return;
                                if (foundOutput.Type != output.Type) return;

                                if (level < minLevel)
                                {
                                    minOutput = foundOutput;
                                    minLevel = level;
                                }
                                if (level > maxLevel)
                                {
                                    maxOutput = foundOutput;
                                    maxLevel = level;
                                }
                            });

                if (minOutput == output && maxOutput != output)
                {
                    if (!teleporterMap.ContainsKey(outputPort))
                    {
                        teleporterMap.Add(outputPort, maxOutput.Pos);
                    }
                    // Need to add copy of same input port but with different teleporter.
                    // Two inputs cause pruning bug.
                }
                else
                {
                    Link.Remove(outputPort);
                }
            }

            Output.AdditionalData.Add(output.Pos, teleporterMap);
        }
    }
}