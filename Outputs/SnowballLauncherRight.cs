using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class SnowballLauncherRight
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int internalX = tile.TileFrameX % 54 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = outputPort.Output.Pos.X - internalX;
            int originY = outputPort.Output.Pos.Y - internalY;
            short typeX = (short)(tile.TileFrameX / 54);
            int frameOffset = 54;
            if (typeX >= 1)
            {
                frameOffset = 0;
            }
            for (int i = originX; i < originX + 3; i++)
            {
                for (int j = originY; j < originY + 3; j++)
                {
                    Main.tile[i, j].TileFrameX = (short)(Main.tile[i, j].TileFrameX + frameOffset);
                }
            }
            NetMessage.SendTileSquare(-1, originX, originY, 3, 3, TileChangeType.None);
        }
    }
}