using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class AnnouncementBox
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num31 = tile.TileFrameX % 36 / 18;
            int num32 = tile.TileFrameY % 36 / 18;
            int num33 = pos.X - num31;
            int num34 = pos.Y - num32;
            if (!Main.AnnouncementBoxDisabled)
            {
                Color pink = Color.Pink;
                int num37 = Sign.ReadSign(num33, num34, false);
                if (num37 != -1 && Main.sign[num37] != null && !string.IsNullOrWhiteSpace(Main.sign[num37].text))
                {
                    if (Main.AnnouncementBoxRange == -1)
                    {
                        if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            Main.NewTextMultiline(Main.sign[num37].text, false, pink, 460);
                            return;
                        }
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.SmartTextMessage, -1, -1, NetworkText.FromLiteral(Main.sign[num37].text), 255, pink.R, pink.G, pink.B, 460, 0, 0);
                            return;
                        }
                    }
                    else if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        if (Main.player[Main.myPlayer].Distance(new Vector2(num33 * 16 + 16, num34 * 16 + 16)) <= Main.AnnouncementBoxRange)
                        {
                            Main.NewTextMultiline(Main.sign[num37].text, false, pink, 460);
                            return;
                        }
                    }
                    else if (Main.netMode == NetmodeID.Server)
                    {
                        for (int num38 = 0; num38 < 255; num38++)
                        {
                            if (Main.player[num38].active && Main.player[num38].Distance(new Vector2(num33 * 16 + 16, num34 * 16 + 16)) <= Main.AnnouncementBoxRange)
                            {
                                NetMessage.SendData(MessageID.SmartTextMessage, num38, -1, NetworkText.FromLiteral(Main.sign[num37].text), 255, pink.R, pink.G, pink.B, 460, 0, 0);
                            }
                        }
                        return;
                    }
                }
            }
        }
    }
}