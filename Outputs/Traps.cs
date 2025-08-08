using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class Traps
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num101 = (int)(tile.TileFrameY / 18);
            Vector2 zero = Vector2.Zero;
            float speedX = 0f;
            float speedY = 0f;
            int num102 = 0;
            int damage2 = 0;
            switch (num101)
            {
                case 0:
                case 1:
                case 2:
                case 5:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 200))
                    {
                        int num103 = (tile.TileFrameX == 0) ? -1 : ((tile.TileFrameX == 18) ? 1 : 0);
                        int num104 = (tile.TileFrameX < 36) ? 0 : ((tile.TileFrameX < 72) ? -1 : 1);
                        zero = new Vector2((float)(pos.X * 16 + 8 + 10 * num103), (float)(pos.Y * 16 + 8 + 10 * num104));
                        float num105 = 3f;
                        if (num101 == 0)
                        {
                            num102 = 98;
                            damage2 = 20;
                            num105 = 12f;
                        }
                        if (num101 == 1)
                        {
                            num102 = 184;
                            damage2 = 40;
                            num105 = 12f;
                        }
                        if (num101 == 2)
                        {
                            num102 = 187;
                            damage2 = 40;
                            num105 = 5f;
                        }
                        if (num101 == 5)
                        {
                            num102 = 980;
                            damage2 = 30;
                            num105 = 12f;
                        }
                        speedX = (float)num103 * num105;
                        speedY = (float)num104 * num105;
                    }
                    break;
                case 3:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 300))
                    {
                        int num106 = 200;
                        for (int num107 = 0; num107 < 1000; num107++)
                        {
                            if (Main.projectile[num107].active && Main.projectile[num107].type == num102)
                            {
                                float num108 = (new Vector2((float)(pos.X * 16 + 8), (float)(pos.Y * 18 + 8)) - Main.projectile[num107].Center).Length();
                                if (num108 < 50f)
                                {
                                    num106 -= 50;
                                }
                                else if (num108 < 100f)
                                {
                                    num106 -= 15;
                                }
                                else if (num108 < 200f)
                                {
                                    num106 -= 10;
                                }
                                else if (num108 < 300f)
                                {
                                    num106 -= 8;
                                }
                                else if (num108 < 400f)
                                {
                                    num106 -= 6;
                                }
                                else if (num108 < 500f)
                                {
                                    num106 -= 5;
                                }
                                else if (num108 < 700f)
                                {
                                    num106 -= 4;
                                }
                                else if (num108 < 900f)
                                {
                                    num106 -= 3;
                                }
                                else if (num108 < 1200f)
                                {
                                    num106 -= 2;
                                }
                                else
                                {
                                    num106--;
                                }
                            }
                        }
                        if (num106 > 0)
                        {
                            num102 = 185;
                            damage2 = 40;
                            int num109 = 0;
                            int num110 = 0;
                            switch (tile.TileFrameX / 18)
                            {
                                case 0:
                                case 1:
                                    num109 = 0;
                                    num110 = 1;
                                    break;
                                case 2:
                                    num109 = 0;
                                    num110 = -1;
                                    break;
                                case 3:
                                    num109 = -1;
                                    num110 = 0;
                                    break;
                                case 4:
                                    num109 = 1;
                                    num110 = 0;
                                    break;
                            }
                            speedX = (float)(4 * num109) + (float)Main.rand.Next(-20 + ((num109 == 1) ? 20 : 0), 21 - ((num109 == -1) ? 20 : 0)) * 0.05f;
                            speedY = (float)(4 * num110) + (float)Main.rand.Next(-20 + ((num110 == 1) ? 20 : 0), 21 - ((num110 == -1) ? 20 : 0)) * 0.05f;
                            zero = new Vector2((float)(pos.X * 16 + 8 + 14 * num109), (float)(pos.Y * 16 + 8 + 14 * num110));
                        }
                    }
                    break;
                case 4:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 90))
                    {
                        int num111 = 0;
                        int num112 = 0;
                        switch (tile.TileFrameX / 18)
                        {
                            case 0:
                            case 1:
                                num111 = 0;
                                num112 = 1;
                                break;
                            case 2:
                                num111 = 0;
                                num112 = -1;
                                break;
                            case 3:
                                num111 = -1;
                                num112 = 0;
                                break;
                            case 4:
                                num111 = 1;
                                num112 = 0;
                                break;
                        }
                        speedX = (float)(8 * num111);
                        speedY = (float)(8 * num112);
                        damage2 = 60;
                        num102 = 186;
                        zero = new Vector2((float)(pos.X * 16 + 8 + 18 * num111), (float)(pos.Y * 16 + 8 + 18 * num112));
                    }
                    break;
            }
            switch (num101 + 10)
            {
                case 0:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 200))
                    {
                        int num113 = -1;
                        if (tile.TileFrameX != 0)
                        {
                            num113 = 1;
                        }
                        speedX = (float)(12 * num113);
                        damage2 = 20;
                        num102 = 98;
                        zero = new Vector2((float)(pos.X * 16 + 8), (float)(pos.Y * 16 + 7));
                        zero.X += (float)(10 * num113);
                        zero.Y += 2f;
                    }
                    break;
                case 1:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 200))
                    {
                        int num114 = -1;
                        if (tile.TileFrameX != 0)
                        {
                            num114 = 1;
                        }
                        speedX = (float)(12 * num114);
                        damage2 = 40;
                        num102 = 184;
                        zero = new Vector2((float)(pos.X * 16 + 8), (float)(pos.Y * 16 + 7));
                        zero.X += (float)(10 * num114);
                        zero.Y += 2f;
                    }
                    break;
                case 2:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 200))
                    {
                        int num115 = -1;
                        if (tile.TileFrameX != 0)
                        {
                            num115 = 1;
                        }
                        speedX = (float)(5 * num115);
                        damage2 = 40;
                        num102 = 187;
                        zero = new Vector2((float)(pos.X * 16 + 8), (float)(pos.Y * 16 + 7));
                        zero.X += (float)(10 * num115);
                        zero.Y += 2f;
                    }
                    break;
                case 3:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 300))
                    {
                        num102 = 185;
                        int num116 = 200;
                        for (int num117 = 0; num117 < 1000; num117++)
                        {
                            if (Main.projectile[num117].active && Main.projectile[num117].type == num102)
                            {
                                float num118 = (new Vector2((float)(pos.X * 16 + 8), (float)(pos.Y * 18 + 8)) - Main.projectile[num117].Center).Length();
                                if (num118 < 50f)
                                {
                                    num116 -= 50;
                                }
                                else if (num118 < 100f)
                                {
                                    num116 -= 15;
                                }
                                else if (num118 < 200f)
                                {
                                    num116 -= 10;
                                }
                                else if (num118 < 300f)
                                {
                                    num116 -= 8;
                                }
                                else if (num118 < 400f)
                                {
                                    num116 -= 6;
                                }
                                else if (num118 < 500f)
                                {
                                    num116 -= 5;
                                }
                                else if (num118 < 700f)
                                {
                                    num116 -= 4;
                                }
                                else if (num118 < 900f)
                                {
                                    num116 -= 3;
                                }
                                else if (num118 < 1200f)
                                {
                                    num116 -= 2;
                                }
                                else
                                {
                                    num116--;
                                }
                            }
                        }
                        if (num116 > 0)
                        {
                            speedX = (float)Main.rand.Next(-20, 21) * 0.05f;
                            speedY = 4f + (float)Main.rand.Next(0, 21) * 0.05f;
                            damage2 = 40;
                            zero = new Vector2((float)(pos.X * 16 + 8), (float)(pos.Y * 16 + 16));
                            zero.Y += 6f;
                            Projectile.NewProjectile(Wiring.GetProjectileSource(pos.X, pos.Y), (float)((int)zero.X), (float)((int)zero.Y), speedX, speedY, num102, damage2, 2f, Main.myPlayer, 0f, 0f, 0f);
                        }
                    }
                    break;
                case 4:
                    if (WiringWrapper.CheckMech(pos.X, pos.Y, 90))
                    {
                        speedX = 0f;
                        speedY = 8f;
                        damage2 = 60;
                        num102 = 186;
                        zero = new Vector2((float)(pos.X * 16 + 8), (float)(pos.Y * 16 + 16));
                        zero.Y += 10f;
                    }
                    break;
            }
            if (num102 != 0)
            {
                Projectile.NewProjectile(Wiring.GetProjectileSource(pos.X, pos.Y), (float)((int)zero.X), (float)((int)zero.Y), speedX, speedY, num102, damage2, 2f, Main.myPlayer, 0f, 0f, 0f);
                return;
            }
        }
    }
}