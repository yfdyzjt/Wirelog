using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class BubbleMachine
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num79;
            for (num79 = tile.TileFrameX / 18; num79 >= 3; num79 -= 3) ;
            int num80;
            for (num80 = tile.TileFrameY / 18; num80 >= 3; num80 -= 3) ;
            int num81 = pos.X - num79;
            int num82 = pos.Y - num80;
            int num83 = 54;
            if (Main.tile[num81, num82].TileFrameX >= 54)
            {
                num83 = -54;
            }
            for (int num84 = num81; num84 < num81 + 3; num84++)
            {
                for (int num85 = num82; num85 < num82 + 2; num85++)
                {
                    Main.tile[num84, num85].TileFrameX = (short)(Main.tile[num84, num85].TileFrameX + num83);
                }
            }
            NetMessage.SendTileSquare(-1, num81, num82, 3, 2, TileChangeType.None);
        }
    }
}