using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class CannonsShot
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int internalX = tile.TileFrameX % 72 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = pos.X - internalX;
            int originY = pos.Y - internalY;
            int typeY = tile.TileFrameY / 54;
            int typeX = tile.TileFrameX / 72;
            if (WiringWrapper.CheckMech(originX, originY, 30))
            {
                WorldGen.ShootFromCannon(originX, originY, typeY, typeX + 1, 0, 0f, WiringWrapper.CurrentUser, true);
            }
        }
    }
}