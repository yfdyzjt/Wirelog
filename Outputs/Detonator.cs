using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Detonator
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int num12 = tile.TileFrameX % 36 / 18;
            int num13 = tile.TileFrameY % 36 / 18;
            int num14 = outputPort.Output.Pos.X - num12;
            int num15 = outputPort.Output.Pos.Y - num13;
            int num16 = 36;
            if (Main.tile[num14, num15].TileFrameX >= 36)
            {
                num16 = -36;
            }
            for (int num17 = num14; num17 < num14 + 2; num17++)
            {
                for (int num18 = num15; num18 < num15 + 2; num18++)
                {
                    Main.tile[num17, num18].TileFrameX = (short)(Main.tile[num17, num18].TileFrameX + num16);
                }
            }
            NetMessage.SendTileSquare(-1, num14, num15, 2, 2, TileChangeType.None);
        }
    }
}