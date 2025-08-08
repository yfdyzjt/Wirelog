using Terraria;

namespace Wirelog.Outputs
{
    public static class Toilets
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int num68 = outputPort.Output.Pos.Y - tile.TileFrameY % 40 / 18;
            if (WiringWrapper.CheckMech(outputPort.Output.Pos.X, num68, 60))
            {
                Projectile.NewProjectile(Wiring.GetProjectileSource(outputPort.Output.Pos.X, num68), outputPort.Output.Pos.X * 16 + 8, num68 * 16 + 12, 0f, 0f, 733, 0, 0f, Main.myPlayer, 0f, 0f, 0f);
                return;
            }
        }
    }
}