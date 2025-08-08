using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class CannonsRight
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            var frameYOffset = -54;
            int internalX = tile.TileFrameX % 72 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = pos.X - internalX;
            int originY = pos.Y - internalY;
            int typeY = tile.TileFrameY / 54;
            if (typeY <= 0)
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