using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class ConveyorBelts
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            if (tile.TileType == 421)
            {
                tile.TileType = 422;
                WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
                NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);

            }
            else
            {
                tile.TileType = 421;
                WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
                NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
            }
        }
    }
}