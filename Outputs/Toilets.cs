using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Toilets
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num68 = pos.Y - tile.TileFrameY % 40 / 18;
            if (WiringWrapper.CheckMech(pos.X, num68, 60))
            {
                Projectile.NewProjectile(Wiring.GetProjectileSource(pos.X, num68), pos.X * 16 + 8, num68 * 16 + 12, 0f, 0f, 733, 0, 0f, Main.myPlayer, 0f, 0f, 0f);
                return;
            }
        }
    }
}