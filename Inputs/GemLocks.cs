using Terraria.Audio;
using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class GemLocks
    {
        public static void Activate(Point16 pos)
        {
            // SoundEngine.PlaySound(28, i * 16 + 16, j * 16 + 16, 0, 1f, 0f);
            Interface.InputActivate(pos);
        }
    }
}