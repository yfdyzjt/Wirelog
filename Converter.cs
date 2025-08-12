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
            Link.Remove(_wires);
            _wires.Clear();
            Link.Remove(_lampsFound.Values);
            _lampsFound.Clear();
            Link.Remove(_gatesFound.Values);
            _gatesFound.Clear();

            _outputsFound.Clear();
            _inputPorts = null;
        }
    }
}
