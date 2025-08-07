using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class OpenDoors
    {
        public static void Activate(Point16 pos)
        {
            var newPos = pos;
            for (int i = -1; i <= 1; i++)
            {
                if (Main.tile[pos.X + i, pos.Y].TileType is 11 or 10)
                {
                    newPos = new Point16(pos.X + i, pos.Y);
                    break;
                }
            }

            if(Main.tile[newPos].TileType == 11)
            {
                if (WorldGen.CloseDoor(newPos.X,newPos.Y, true))
                {
                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 1, newPos.X, newPos.Y, 0f, 0, 0, 0);
                    return;
                }

            }
            else if(Main.tile[newPos].TileType == 10)
            {
                int num67 = 1;
                if (Main.rand.NextBool(2))
                {
                    num67 = -1;
                }
                if (WorldGen.OpenDoor(newPos.X, newPos.Y, num67))
                {
                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, newPos.X, newPos.Y, (float)num67, 0, 0, 0);
                    return;
                }
                if (WorldGen.OpenDoor(newPos.X, newPos.Y, -num67))
                {
                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, newPos.X, newPos.Y, (float)(-(float)num67), 0, 0, 0);
                    return;
                }
            }
        }
    }
}