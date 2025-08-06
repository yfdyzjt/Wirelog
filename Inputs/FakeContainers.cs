using Terraria.Audio;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class FakeContainers
    {
        public static void Activate(Point16 pos)
        {
            int num = Main.tile[pos].TileFrameX / 18 * -1;
            int num2 = Main.tile[pos].TileFrameX / 18 * -1;
            num %= 4;
            if (num < -1)
            {
                num += 2;
            }
            num += pos.X;
            num2 += pos.Y;
            // SoundEngine.PlaySound(28, i * 16, j * 16, 0, 1f, 0f);
            Interface.InputActivate(new Point16(num, num2));
        }
    }
}