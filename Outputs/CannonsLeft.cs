using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class CannonsLeft
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            var frameYOffset = 54;
            int internalX = tile.TileFrameX % 72 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = outputPort.Output.Pos.X - internalX;
            int originY = outputPort.Output.Pos.Y - internalY;
            int typeY = tile.TileFrameY / 54;
            if (typeY >= 8)
            {
                frameYOffset = 0;
            }
            for (int i = originX; i < originX + 4; i++)
            {
                for (int j = originY; j < originY + 3; j++)
                {
                    Main.tile[i, j].TileFrameY = (short)(Main.tile[i, j].TileFrameY + frameYOffset);
                }
            }
            NetMessage.SendTileSquare(-1, originX, originY, 4, 3, TileChangeType.None);
        }
    }
}