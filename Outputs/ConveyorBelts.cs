using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class ConveyorBelts
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            if (tile.TileType == 421)
            {
                tile.TileType = 422;
                WorldGen.SquareTileFrame(pos.X, pos.Y, true);
                NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);

            }
            else
            {
                tile.TileType = 421;
                WorldGen.SquareTileFrame(pos.X, pos.Y, true);
                NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
            }
        }
    }
}