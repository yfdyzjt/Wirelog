using Terraria;
using Terraria.DataStructures;

namespace Wirelog
{
    public static class Interface
    {
        public static void InputActivate(Point16 pos)
        {
            if (TryGetInput(pos, out var input))
            {
                int inputPortId = input.InputPort.Id;
                var outputIds = VerilogSimulator.SendInputAndWaitForOutput(inputPortId);

                foreach (var outputId in outputIds)
                {
                    if (TryGetOutput(outputId, out var output))
                    {
                        Output.Activate(output.Type, output.Pos);
                    }
                }
            }
        }

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

        private static bool TryGetOutput(int outputId, out Output outputResult)
        {
            if (Converter.OutputsPortFound.TryGetValue(outputId, out var outputPort))
            {
                outputResult = outputPort.Output;
                return true;
            }
            outputResult = null;
            return false;
        }
    }
}
