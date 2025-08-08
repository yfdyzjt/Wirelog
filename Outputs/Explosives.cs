using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Explosives
    {
        public static void Activate(Point16 pos)
        {
            WorldGen.KillTile(pos.X, pos.Y, false, false, true);
            NetMessage.SendTileSquare(-1, pos.X, pos.Y, TileChangeType.None);
            Projectile.NewProjectile(Wiring.GetProjectileSource(pos.X, pos.Y), pos.X * 16 + 8, pos.Y * 16 + 8, 0f, 0f, 108, 500, 10f, Main.myPlayer, 0f, 0f, 0f);
        }
    }
}