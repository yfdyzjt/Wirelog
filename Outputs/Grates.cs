using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Grates
    {
        public static void Activate(OutputPort outputPort)
        {
            if (Main.tile[outputPort.Output.Pos].TileType == 546)
            {
                Main.tile[outputPort.Output.Pos].TileType = 557;
            }
            else
            {
                Main.tile[outputPort.Output.Pos].TileType = 546;
            }
            WorldGen.SquareTileFrame(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, true);
            NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
        }
    }
}