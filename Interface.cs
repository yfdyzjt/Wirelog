using Terraria;

namespace Wirelog
{
    public static class Interface
    {
        public static void InputActivate(Input input)
        {
            Main.NewText($"{input.InputPort.Id}");
        }
    }
}
