using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Chimney
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int num2 = tile.TileFrameX % 54 / 18;
            int num3 = tile.TileFrameY % 54 / 18;
            int num4 = outputPort.Output.Pos.X - num2;
            int num5 = outputPort.Output.Pos.Y - num3;
            int num6 = 54;
            if (Main.tile[num4, num5].TileFrameY >= 108)
            {
                num6 = -108;
            }
            for (int k = num4; k < num4 + 3; k++)
            {
                for (int l = num5; l < num5 + 3; l++)
                {
                    Main.tile[k, l].TileFrameY = (short)(Main.tile[k, l].TileFrameY + num6);
                }
            }
            NetMessage.SendTileSquare(-1, num4 + 1, num5 + 1, 3, TileChangeType.None);
        }
    }
}