using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class SunAndMoondial
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num19 = tile.TileFrameX % 36 / 18;
            int num20 = tile.TileFrameY % 54 / 18;
            int num21 = pos.X - num19;
            int num22 = pos.Y - num20;
            if (tile.TileType == 356)
            {
                if (!Main.fastForwardTimeToDawn && Main.sundialCooldown == 0)
                {
                    Main.Sundialing();
                }
            }
            else if (tile.TileType == 663)
            {
                if (!Main.fastForwardTimeToDusk && Main.moondialCooldown == 0)
                {
                    Main.Moondialing();
                }
            }
            NetMessage.SendTileSquare(-1, num21, num22, 2, 2, TileChangeType.None);
        }
    }
}