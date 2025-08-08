using Terraria;
using Terraria.DataStructures;

namespace Wirelog.Outputs
{
    public static class FireworksBox
    {
        public static void Activate(Point16 pos)
        {
            var tile = Main.tile[pos];
            int num69 = pos.Y - tile.TileFrameY / 18;
            int num70 = pos.X - tile.TileFrameX / 18;
            if (WiringWrapper.CheckMech(num70, num69, 30))
            {
                WorldGen.LaunchRocketSmall(num70, num69, true);
            }
        }
    }
}