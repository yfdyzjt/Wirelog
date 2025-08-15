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

        private static readonly Dictionary<long, Module> _moduleDefinitions = [];
        private static readonly List<ModuleInstance> _moduleInstances = [];

        private static InputPort[] _inputPorts;
        private static OutputPort[] _outputPorts;

        private static readonly List<Wire> _wires = [];

        public static Dictionary<Point16, Input> InputsFound => _inputsFound;
        public static Dictionary<Point16, Gate> GatesFound => _gatesFound;
        public static Dictionary<Point16, Output> OutputsFound => _outputsFound;

        public static Dictionary<Point16, InputPort> InputsPortFound { get; } = [];
        public static OutputPort[] OutputsPortFound => _outputPorts;

        public static void Do()
        {
            LoadVModules();
            PreClear();
            Preprocess();
            Postprocess();
            VerilogConvert();
            PostClear();
        }
    }
}
