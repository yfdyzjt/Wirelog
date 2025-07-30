using System.Collections.Generic;
using Terraria.DataStructures;

namespace Wirelog
{
    public partial class Converter
    {
        private static readonly Dictionary<Point16, Input> _inputsFound = [];
        private static readonly Dictionary<Point16, Output> _outputsFound = [];
        private static readonly Dictionary<Point16, Gate> _gatesFound = [];
        private static readonly Dictionary<Point16, Lamp> _lampsFound = [];

        private static readonly List<Wire> _wires = [];

        private static readonly Dictionary<int, InputPort> _inputsPortFound = [];
        private static readonly Dictionary<int, OutputPort> _outputsPortFound = [];

    }
}
