using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class PressurePlates
    {
        public static void Activate(Point16 pos)
        {
            // SoundEngine.PlaySound(28, i * 16, j * 16, 0, 1f, 0f);
            Interface.InputActivate(pos);
        }
    }
}