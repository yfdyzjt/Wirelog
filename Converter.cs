using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog
{
    public static partial class Converter
    {
        private static readonly Dictionary<Point16, Gate> _gatesFound = [];
        private static readonly Dictionary<Point16, Lamp> _lampsFound = [];
        private static readonly Dictionary<Point16, Input> _inputsFound = [];
        private static readonly Dictionary<Point16, Output> _outputsFound = [];

        private static readonly Dictionary<int, InputPort> _inputsPortFound = [];
        private static readonly Dictionary<int, OutputPort> _outputsPortFound = [];

        private static readonly List<Wire> _wires = [];

        public static void Convert()
        {
            LoadVModules();
            AllClear();
            Preprocess();
            Postprocess();
            VerilogConvert();
        }

        public static bool TryFoundInput(Point16 pos, out Input foundInput)
        {
            if (Input.TryGetType(Main.tile[pos], out var inputType))
            {
                var (sizeX, sizeY) = Input.GetSize(inputType);
                for (int dX = 0; dX < sizeX; dX++)
                {
                    for (int dY = 0; dY < sizeY; dY++)
                    {
                        var curPos = new Point16(pos.X + dX, pos.Y + dY);
                        if (_inputsFound.TryGetValue(curPos, out var input))
                        {
                            foundInput = input;
                            return true;
                        }
                    }
                }
            }
            foundInput = null;
            return false;
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
