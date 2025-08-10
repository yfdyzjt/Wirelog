using System.Collections.Generic;
using Terraria.DataStructures;

namespace Wirelog
{
    public static partial class Converter
    {
        private static readonly Dictionary<Point16, Gate> _gatesFound = [];
        private static readonly Dictionary<Point16, Lamp> _lampsFound = [];
        private static readonly Dictionary<Point16, Input> _inputsFound = [];
        private static readonly Dictionary<Point16, Output> _outputsFound = [];

        private static InputPort[] _inputPorts;
        private static OutputPort[] _outputPorts;

        private static readonly List<Wire> _wires = [];

        public static Dictionary<Point16, Input> InputsFound => _inputsFound;
        public static Dictionary<Point16, Gate> GatesFound => _gatesFound;
        public static Dictionary<Point16, Output> OutputsFound => _outputsFound;

        public static Dictionary<Point16, InputPort> InputsPortFound { get; } = [];
        public static OutputPort[] OutputsPortFound => _outputPorts;

        public static void Convert()
        {
            LoadVModules();
            PreClear();
            Preprocess();
            Postprocess();
            VerilogConvert();
            PostClear();
        }

        private static void PreClear()
        {
            _inputsFound.Clear();
            _outputsFound.Clear();
            _gatesFound.Clear();
            _lampsFound.Clear();
            _wires.Clear();
            _inputPorts = null;
            _outputPorts = null;
            InputsPortFound.Clear();
        }

        private static void PostClear()
        {
            foreach (var wire in _wires)
            {
                wire.InputPorts.Clear();
                wire.OutputPorts.Clear();
                wire.Gates.Clear();
                wire.Lamps.Clear();
            }
            foreach (var lamp in _lampsFound.Values)
            {
                lamp.OutputGate = null;
                lamp.InputWires.Clear();
            }
            foreach (var gate in _gatesFound.Values)
            {
                gate.InputLamps.Clear();
                gate.OutputWires.Clear();
            }
            foreach (var inputPort in _inputPorts)
            {
                inputPort.OutputWires.Clear();
            }
            foreach (var outputPort in _outputPorts)
            {
                outputPort.InputWire = null;
            }
            _outputsFound.Clear();
            _gatesFound.Clear();
            _lampsFound.Clear();
            _wires.Clear();
            _inputPorts = null;
        }
    }
}
