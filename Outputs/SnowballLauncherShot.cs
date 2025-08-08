using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Wirelog.Outputs
{
    public static class SnowballLauncherShot
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int internalX = tile.TileFrameX % 54 / 18;
            int internalY = tile.TileFrameY % 54 / 18;
            int originX = outputPort.Output.Pos.X - internalX;
            int originY = outputPort.Output.Pos.Y - internalY;
            if (WiringWrapper.CheckMech(originX, originY, 10))
            {
                float num60 = 12f + Main.rand.Next(450) * 0.01f;
                float num61 = Main.rand.Next(85, 105);
                float num62 = Main.rand.Next(-35, 11);
                int type2 = 166;
                int damage = 0;
                float knockBack = 0f;
                Vector2 vector = new((originX + 2) * 16 - 8, (originY + 2) * 16 - 8);
                if (tile.TileFrameX / 54 == 0)
                {
                    num61 *= -1f;
                    vector.X -= 12f;
                }
                else
                {
                    vector.X += 12f;
                }
                float num63 = num61;
                float num64 = num62;
                float num65 = (float)Math.Sqrt((double)(num63 * num63 + num64 * num64));
                num65 = num60 / num65;
                num63 *= num65;
                num64 *= num65;
                Projectile.NewProjectile(Wiring.GetProjectileSource(originX, originY), vector.X, vector.Y, num63, num64, type2, damage, knockBack, WiringWrapper.CurrentUser, 0f, 0f, 0f);
            }
        }
    }
}