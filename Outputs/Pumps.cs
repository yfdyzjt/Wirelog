using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Pumps
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            if (tile.TileType == TileID.OutletPump) return;

            var inletPump = outputPort;
            if (Output.AdditionalData.TryGetValue(outputPort.Output.Pos, out var obj))
            {
                var pumpMap = (Dictionary<OutputPort, HashSet<OutputPort>>)obj;
                if (pumpMap.TryGetValue(inletPump, out var outletPumps))
                {
                    WiringWrapper.InPump.Clear();
                    WiringWrapper.OutPump.Clear();

                    int num124 = inletPump.Output.Pos.Y - tile.TileFrameY / 18;
                    int num125 = tile.TileFrameX / 18;
                    if (num125 > 1)
                    {
                        num125 -= 2;
                    }
                    num125 = inletPump.Output.Pos.X - num125;

                    for (int num126 = 0; num126 < 4; num126++)
                    {
                        if (WiringWrapper.NumInPump >= 19)
                        {
                            break;
                        }
                        int num127;
                        int num128;
                        if (num126 == 0)
                        {
                            num127 = num125;
                            num128 = num124 + 1;
                        }
                        else if (num126 == 1)
                        {
                            num127 = num125 + 1;
                            num128 = num124 + 1;
                        }
                        else if (num126 == 2)
                        {
                            num127 = num125;
                            num128 = num124;
                        }
                        else
                        {
                            num127 = num125 + 1;
                            num128 = num124;
                        }
                        WiringWrapper.InPump.Add(new Point16(num127, num128));
                    }

                    foreach(var outletPump in outletPumps)
                    {
                        tile = Main.tile[outletPump.Output.Pos];
                        num124 = outletPump.Output.Pos.Y - tile.TileFrameY / 18;
                        num125 = tile.TileFrameX / 18;
                        if (num125 > 1)
                        {
                            num125 -= 2;
                        }
                        num125 = outletPump.Output.Pos.X - num125;

                        for (int num129 = 0; num129 < 4; num129++)
                        {
                            if (WiringWrapper.NumOutPump >= 19)
                            {
                                break;
                            }
                            int num127;
                            int num128;
                            if (num129 == 0)
                            {
                                num127 = num125;
                                num128 = num124 + 1;
                            }
                            else if (num129 == 1)
                            {
                                num127 = num125 + 1;
                                num128 = num124 + 1;
                            }
                            else if (num129 == 2)
                            {
                                num127 = num125;
                                num128 = num124;
                            }
                            else
                            {
                                num127 = num125 + 1;
                                num128 = num124;
                            }
                            WiringWrapper.OutPump.Add(new Point16(num127, num128));
                        }
                    }

                    WiringWrapper.XferWater();
                }
            }
        }

        public static void Postprocess(Output output)
        {
            var tile = Main.tile[output.Pos];
            if (tile.TileType == TileID.OutletPump) return;

            var pumpMap = new Dictionary<OutputPort, HashSet<OutputPort>>();
            foreach (var inletPump in output.OutputPorts)
            {
                HashSet<OutputPort> outletPumps = [];
                foreach (var outletPump in inletPump.InputWire.OutputPorts.Where(o =>
                Main.tile[o.Output.Pos].TileType == TileID.OutletPump))
                {
                    outletPumps.Add(outletPump);
                }
                pumpMap.Add(inletPump, outletPumps);
            }
            Output.AdditionalData.Add(output.Pos, pumpMap);
        }
    }
}