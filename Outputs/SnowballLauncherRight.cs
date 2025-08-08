using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class SnowballLauncherRight
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int internalX = tile.TileFrameX % 54 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = pos.X - internalX;
            int originY = pos.Y - internalY;
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