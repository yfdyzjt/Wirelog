using System.Collections.Generic;
using Terraria.ID;
using Terraria;

namespace Wirelog.Outputs
{
    public static class WireBulb
    {
        public static void Activate(OutputPort outputPort)
        {
            if (Output.AdditionalData.TryGetValue(outputPort.Output.Pos, out var obj))
            {
                var outputPortColorMap = (Dictionary<OutputPort, WireType>)obj;
                if (outputPortColorMap.TryGetValue(outputPort, out var wireColor))
                {
                    var tile = Main.tile[outputPort.Output.Pos.X, outputPort.Output.Pos.Y];
                    short num77 = (short)(tile.TileFrameX / 18);
                    bool flag6 = num77 % 2 >= 1;
                    bool flag7 = num77 % 4 >= 2;
                    bool flag8 = num77 % 8 >= 4;
                    bool flag9 = num77 % 16 >= 8;
                    bool flag10 = false;
                    short num78 = 0;
                    switch (wireColor)
                    {
                        case WireType.Red:
                            num78 = 18;
                            flag10 = !flag6;
                            break;
                        case WireType.Blue:
                            num78 = 72;
                            flag10 = !flag8;
                            break;
                        case WireType.Green:
                            num78 = 36;
                            flag10 = !flag7;
                            break;
                        case WireType.Yellow:
                            num78 = 144;
                            flag10 = !flag9;
                            break;
                    }
                    if (flag10)
                    {
                        tile.TileFrameX += num78;
                    }
                    else
                    {
                        tile.TileFrameX -= num78;
                    }
                    NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
                }
            }
        }

        public static void Postprocess(Output output)
        {
            var outputPortColorMap = new Dictionary<OutputPort, WireType>();
            foreach (var outputPort in output.OutputPorts)
            {
                outputPortColorMap.Add(outputPort, outputPort.Wire.Type);
            }
            Output.AdditionalData.Add(output.Pos, outputPortColorMap);
        }
    }
}