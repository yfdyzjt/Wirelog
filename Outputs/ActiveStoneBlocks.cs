using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class ActiveStoneBlocks
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            if (tile.TileType == 130)
            {
                if (Main.tile[outputPort.Output.Pos.X, outputPort.Output.Pos.Y - 1] == null || 
                    !Main.tile[outputPort.Output.Pos.X, outputPort.Output.Pos.Y - 1].HasTile || 
                    (!TileID.Sets.BasicChest[Main.tile[outputPort.Output.Pos.X, outputPort.Output.Pos.Y - 1].TileType] && 
                    !TileID.Sets.BasicChestFake[Main.tile[outputPort.Output.Pos.X, outputPort.Output.Pos.Y - 1].TileType] && 
                    Main.tile[outputPort.Output.Pos.X, outputPort.Output.Pos.Y - 1].TileType != 88))
                {
                    tile.TileType = 131;
                    WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
                    NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
                }
            }
            else
            {
                tile.TileType = 130;
                WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
                NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
            }
        }
    }
}