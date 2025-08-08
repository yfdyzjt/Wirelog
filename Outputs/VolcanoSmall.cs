using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class VolcanoSmall
    {
        public static void Activate(OutputPort outputPort)
        {
            short num93;
            if (Main.tile[outputPort.Output.Pos].TileFrameX == 0)
            {
                num93 = 18;
            }
            else
            {
                num93 = -18;
            }
            Main.tile[outputPort.Output.Pos].TileFrameX += num93;
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, 1, 1, TileChangeType.None);
            }
            int num94 = (num93 > 0) ? 4 : 3;
            Animation.NewTemporaryAnimation(num94, 593, outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
            NetMessage.SendTemporaryAnimation(-1, num94, 593, outputPort.Output.Pos.X, outputPort.Output.Pos.Y);
        }
    }
}