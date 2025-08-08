using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class FogMachine
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int num86;
            for (num86 = tile.TileFrameX / 18; num86 >= 2; num86 -= 2) ;
            int num87;
            for (num87 = tile.TileFrameY / 18; num87 >= 2; num87 -= 2) ;
            int num88 = outputPort.Output.Pos.X - num86;
            int num89 = outputPort.Output.Pos.Y - num87;
            int num90 = 36;
            if (Main.tile[num88, num89].TileFrameX >= 36)
            {
                num90 = -36;
            }
            for (int num91 = num88; num91 < num88 + 2; num91++)
            {
                for (int num92 = num89; num92 < num89 + 2; num92++)
                {
                    Main.tile[num91, num92].TileFrameX = (short)(Main.tile[num91, num92].TileFrameX + num90);
                }
            }
            NetMessage.SendTileSquare(-1, num88, num89, 2, 2, TileChangeType.None);
        }
    }
}