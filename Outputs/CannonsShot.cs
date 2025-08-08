using Terraria;

namespace Wirelog.Outputs
{
    public static class CannonsShot
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int internalX = tile.TileFrameX % 72 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = outputPort.Output.Pos.X - internalX;
            int originY = outputPort.Output.Pos.Y - internalY;
            int typeY = tile.TileFrameY / 54;
            int typeX = tile.TileFrameX / 72;
            if (WiringWrapper.CheckMech(originX, originY, 30))
            {
                WorldGen.ShootFromCannon(originX, originY, typeY, typeX + 1, 0, 0f, WiringWrapper.CurrentUser, true);
            }
        }
    }
}