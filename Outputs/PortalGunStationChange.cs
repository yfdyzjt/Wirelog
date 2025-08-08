using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class PortalGunStationChange
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int internalX = tile.TileFrameX % 72 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = pos.X - internalX;
            int originY = pos.Y - internalY;
            int typeX = tile.TileFrameX / 72;
            var frameXOffset = (typeX == 3) ? 72 : -72;
            for (int i = originX; i < originX + 4; i++)
            {
                for (int j = originY; j < originY + 3; j++)
                {
                    Main.tile[i, j].TileFrameX = (short)(Main.tile[i, j].TileFrameX + frameXOffset);
                }
            }
            NetMessage.SendTileSquare(-1, originX, originY, 4, 3, TileChangeType.None);
        }
    }
}