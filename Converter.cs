using System;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Wirelog
{
    public static partial class Converter
    {
        private static readonly Dictionary<Point16, Input> _inputsFound = [];
        private static readonly Dictionary<Point16, Output> _outputsFound = [];
        private static readonly Dictionary<Point16, Gate> _gatesFound = [];
        private static readonly Dictionary<Point16, Lamp> _lampsFound = [];

        private static readonly List<Wire> _wires = [];

        private static readonly Dictionary<int, InputPort> _inputsPortFound = [];
        private static readonly Dictionary<int, OutputPort> _outputsPortFound = [];

        public static void Convert()
        {
            LoadVModules();
            AllClear();
            Preprocess();
            Postprocess();
            VerilogConvert();
        }

        private static void AllClear()
        {
            _inputsFound.Clear();
            _outputsFound.Clear();
            _gatesFound.Clear();
            _lampsFound.Clear();
            _wires.Clear();
            _inputsPortFound.Clear();
            _outputsPortFound.Clear();
        }
    }
}
