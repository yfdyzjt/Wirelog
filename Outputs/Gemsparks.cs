using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Gemsparks
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            if (tile.TileType >= 262)
            {
                tile.TileType -= 7;
            }
            else
            {
                tile.TileType += 7;
            }
            WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
            NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
        }
    }
}