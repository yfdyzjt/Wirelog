using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class VolcanoLarge
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int num95;
            for (num95 = tile.TileFrameY / 18; num95 >= 2; num95 -= 2)
            {
            }
            num95 = outputPort.Output.Pos.Y - num95;
            int num96 = tile.TileFrameX / 18;
            if (num96 > 1)
            {
                num96 -= 2;
            }
            num96 = outputPort.Output.Pos.X - num96;
            Wiring.SkipWire(num96, num95);
            Wiring.SkipWire(num96, num95 + 1);
            Wiring.SkipWire(num96 + 1, num95);
            Wiring.SkipWire(num96 + 1, num95 + 1);
            short num97;
            if (Main.tile[num96, num95].TileFrameX == 0)
            {
                num97 = 36;
            }
            else
            {
                num97 = -36;
            }
            for (int num98 = 0; num98 < 2; num98++)
            {
                for (int num99 = 0; num99 < 2; num99++)
                {
                    Tile tile7 = Main.tile[num96 + num98, num95 + num99];
                    tile7.TileFrameX += num97;
                }
            }
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, num96, num95, 2, 2, TileChangeType.None);
            }
            int num100 = (num97 > 0) ? 4 : 3;
            Animation.NewTemporaryAnimation(num100, 594, num96, num95);
            NetMessage.SendTemporaryAnimation(-1, num100, 594, num96, num95);
        }
    }
}