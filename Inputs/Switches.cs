using Terraria;

namespace Wirelog.Inputs
{
    public static class Switches
    {
        public static void Activate(Input input)
        {
            Main.NewText($"Switch at {input.Pos}");
        }
    }
}