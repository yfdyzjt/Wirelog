using Terraria;

namespace Wirelog.Outputs
{
    public static class FireworkFountain
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            int num71 = outputPort.Output.Pos.Y - tile.TileFrameY / 18;
            int num72 = outputPort.Output.Pos.X - tile.TileFrameX / 18;
            if (WiringWrapper.CheckMech(num72, num71, 30))
            {
                bool flag5 = false;
                for (int num73 = 0; num73 < 1000; num73++)
                {
                    if (Main.projectile[num73].active && Main.projectile[num73].aiStyle == 73 && Main.projectile[num73].ai[0] == (float)num72 && Main.projectile[num73].ai[1] == (float)num71)
                    {
                        flag5 = true;
                        break;
                    }
                }
                if (!flag5)
                {
                    int type3 = 419 + Main.rand.Next(4);
                    Projectile.NewProjectile(Wiring.GetProjectileSource(num72, num71), num72 * 16 + 8, num71 * 16 + 2, 0f, 0f, type3, 0, 0f, Main.myPlayer, (float)num72, (float)num71, 0f);
                    return;
                }
            }
        }
    }
}