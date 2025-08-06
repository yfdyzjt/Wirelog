using Terraria.DataStructures;

namespace Wirelog.Inputs
{
    public static class ProjectilePressurePad
    {
        public static void Activate(Point16 pos) => PressurePlates.Activate(pos);
    }
}