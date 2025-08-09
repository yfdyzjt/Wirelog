using System.Linq;
using Terraria.DataStructures;
using Terraria;
using System.Collections.Generic;
using System;

namespace Wirelog.Outputs
{
    public static class PixelBox
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            tile.TileFrameX = tile.TileFrameX == 0 ? (short)18 : (short)0;
        }

        public static void Postprocess(Output output)
        {
            HashSet<Gate> linkGates = [];
            HashSet<InputPort> linkInputPorts = [];

            HashSet<Gate> upDownGates = [];
            HashSet<InputPort> upDownInputPorts = [];
            HashSet<Gate> leftRightGates = [];
            HashSet<InputPort> leftRightInputPorts = [];

            foreach (var outpotPort in output.OutputPorts)
            {
                if (outpotPort.InputWire.Gates.Count != 0)
                {
                    linkGates.Add(outpotPort.InputWire.Gates.First());
                }
                else if (outpotPort.InputWire.InputPorts.Count != 0)
                {
                    linkInputPorts.Add(outpotPort.InputWire.InputPorts.First());
                }
            }

            foreach (WireType wireType in Enum.GetValues(typeof(WireType)))
            {
                if (!Wire.HasWire(output.Pos, wireType)) continue;

                var wire = new Wire() { Type = wireType };
                var centerPos = output.Pos;

                TraceSource(new Point16(centerPos.X - 1, centerPos.Y), centerPos, leftRightGates, leftRightInputPorts, wire, linkGates, linkInputPorts);
                TraceSource(new Point16(centerPos.X + 1, centerPos.Y), centerPos, leftRightGates, leftRightInputPorts, wire, linkGates, linkInputPorts);
                TraceSource(new Point16(centerPos.X, centerPos.Y - 1), centerPos, upDownGates, upDownInputPorts, wire, linkGates, linkInputPorts);
                TraceSource(new Point16(centerPos.X, centerPos.Y + 1), centerPos, upDownGates, upDownInputPorts, wire, linkGates, linkInputPorts);
            }

            var doubleDirGates = leftRightGates.Intersect(upDownGates).ToHashSet();
            var doubleDirInputPorts = leftRightInputPorts.Intersect(upDownInputPorts).ToHashSet();
            var singleDirGates = leftRightGates.Except(upDownGates).Union(upDownGates.Except(leftRightGates)).ToHashSet();
            var singleDirInputPorts = leftRightInputPorts.Except(upDownInputPorts).Union(upDownInputPorts.Except(leftRightInputPorts)).ToHashSet();

            foreach (var gate in doubleDirGates)
            {
                var outputPorts = output.OutputPorts.Where(outputPort =>
                outputPort.InputWire.Gates.Contains(gate)).ToHashSet();
                AddNewOutputPort(outputPorts);
            }
            foreach (var inputPort in doubleDirInputPorts)
            {
                var outputPorts = output.OutputPorts.Where(outputPort =>
                outputPort.InputWire.InputPorts.Contains(inputPort)).ToHashSet();
                AddNewOutputPort(outputPorts);
            }
            foreach (var gate in singleDirGates)
            {
                var outputPorts = output.OutputPorts.Where(outputPort =>
                outputPort.InputWire.Gates.Contains(gate)).ToHashSet();
                RemoveOutputPort(outputPorts);
            }
            foreach (var inputPort in singleDirInputPorts)
            {
                var outputPorts = output.OutputPorts.Where(outputPort =>
                outputPort.InputWire.InputPorts.Contains(inputPort)).ToHashSet();
                RemoveOutputPort(outputPorts);
            }
        }

        private static void TraceSource(
            Point16 nextPos, 
            Point16 centerPos,
            HashSet<Gate> gateSet, 
            HashSet<InputPort> inputSet,
            Wire wire, 
            HashSet<Gate> linkGates, 
            HashSet<InputPort> linkInputPorts)
        {
            if (!Wire.HasWire(nextPos, wire.Type)) return;

            var visitedWires = new HashSet<(Point16, WireType)>();
            Converter.PublicTraceWire(wire, nextPos, centerPos, 0, visitedWires,
                (wire, curPos, level) =>
                {
                    if (Converter.GatesFound.TryGetValue(curPos, out var gate))
                    {
                        if (linkGates.Contains(gate))
                        {
                            gateSet.Add(gate);
                        }
                    }
                    else if (Converter.InputsFound.TryGetValue(curPos, out var input))
                    {
                        if (linkInputPorts.Contains(input.InputPort))
                        {
                            inputSet.Add(input.InputPort);
                        }
                    }
                });
        }

        private static void RemoveOutputPort(IEnumerable<OutputPort> outputPorts)
        {
            foreach (var outputPort in outputPorts)
            {
                outputPort.Output.OutputPorts.Remove(outputPort);
                outputPort.Output = null;
                outputPort.InputWire.OutputPorts.Remove(outputPort);
                outputPort.InputWire = null;
            }
        }

        private static void AddNewOutputPort(IEnumerable<OutputPort> outputPorts)
        {
            var firstOutputPort = outputPorts.First();
            var newOutputPort = new OutputPort()
            {
                InputWire = firstOutputPort.InputWire,
                Output = firstOutputPort.Output,
            };
            newOutputPort.InputWire.OutputPorts.Add(newOutputPort);
            newOutputPort.Output.OutputPorts.Add(newOutputPort);
            RemoveOutputPort(outputPorts);
        }
    }
}