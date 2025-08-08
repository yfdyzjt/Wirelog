using Terraria;

namespace Wirelog.Outputs
{
    public static class FireworksBox
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int num69 = outputPort.Output.Pos.Y - tile.TileFrameY / 18;
            int num70 = outputPort.Output.Pos.X - tile.TileFrameX / 18;
            if (WiringWrapper.CheckMech(num70, num69, 30))
            {
                WorldGen.LaunchRocketSmall(num70, num69, true);
            }
        }
    }
}