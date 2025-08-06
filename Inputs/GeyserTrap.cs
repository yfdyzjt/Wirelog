using System.Numerics;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class GeyserTrap
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num = tile.TileFrameX / 36;
            int num2 = pos.X - (tile.TileFrameX - num * 36) / 18;
            if (Wiring.CheckMech(num2, pos.Y, 200))
            {
                int num3 = 654;
                int damage = 20;
                Vector2 zero;
                Vector2 vector;
                if (num < 2)
                {
                    vector = new Vector2((float)(num2 + 1), (float)pos.Y) * 16f;
                    zero = new Vector2(0f, -8f);
                }
                else
                {
                    vector = new Vector2((float)(num2 + 1), (float)(pos.Y + 1)) * 16f;
                    zero = new Vector2(0f, 8f);
                }
                if (num3 != 0)
                {
                    Projectile.NewProjectile(Wiring.GetProjectileSource(num2, pos.Y), (float)((int)vector.X), (float)((int)vector.Y), zero.X, zero.Y, num3, damage, 2f, Main.myPlayer, 0f, 0f, 0f);
                }
            }
        }
    }
}