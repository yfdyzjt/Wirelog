using Terraria.DataStructures;

namespace Wirelog
{
    public static class Interface
    {
        public static void InputActivate(Point16 pos)
        {
            if (Converter.InputsPortFound.TryGetValue(pos, out var inputPort))
            {
                int inputPortId = inputPort.Id;
                VerilogSimulator.EnqueueInput(inputPortId);
            }
        }

        public static void OutputsActivate()
        {
            foreach (var outputPortId in VerilogSimulator.LastFrameOutputs)
            {
                var outputPort = Converter.OutputsPortFound[outputPortId];
                Output.Activate(outputPort.Output.Type, outputPort);
            }
        }

        /*
        private static bool TryGetInput(Point16 pos, out Input inputResult)
        {
            if (Input.TryGetType(Main.tile[pos], out var inputType))
            {
                var (sizeX, sizeY) = Input.GetSize(inputType);
                for (int dX = 0; dX < sizeX; dX++)
                {
                    for (int dY = 0; dY < sizeY; dY++)
                    {
                        var curPos = new Point16(pos.X + dX, pos.Y + dY);
                        if (Converter.InputsFound.TryGetValue(curPos, out var input))
                        {
                            inputResult = input;
                            return true;
                        }
                    }
                }
            }
            inputResult = null;
            return false;
        }
        */
    }
}
