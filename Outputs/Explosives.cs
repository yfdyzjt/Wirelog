using Terraria;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Explosives
    {
        public static void Activate(OutputPort outputPort)
        {
            WorldGen.KillTile(outputPort.Output.Pos.X, outputPort.Output.Pos.Y, false, false, true);
            NetMessage.SendTileSquare(-1, outputPort.Output.Pos.X, outputPort.Output.Pos.Y, TileChangeType.None);
            Projectile.NewProjectile(Wiring.GetProjectileSource(outputPort.Output.Pos.X, outputPort.Output.Pos.Y), outputPort.Output.Pos.X * 16 + 8, outputPort.Output.Pos.Y * 16 + 8, 0f, 0f, 108, 500, 10f, Main.myPlayer, 0f, 0f, 0f);
        }
    }
}