using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class ActiveStoneBlocks
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            if (tile.TileType == 130)
            {
                if (Main.tile[pos.X, pos.Y - 1] == null || 
                    !Main.tile[pos.X, pos.Y - 1].HasTile || 
                    (!TileID.Sets.BasicChest[Main.tile[pos.X, pos.Y - 1].TileType] && 
                    !TileID.Sets.BasicChestFake[Main.tile[pos.X, pos.Y - 1].TileType] && 
                    Main.tile[pos.X, pos.Y - 1].TileType != 88))
                {
                    tile.TileType = 131;
                    WorldGen.SquareTileFrame(pos.X, pos.Y, true);
                    NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
                }
            }
            else
            {
                tile.TileType = 130;
                WorldGen.SquareTileFrame(pos.X, pos.Y, true);
                NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
            }
        }
    }
}