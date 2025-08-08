using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog
{
    internal class WiringWrapper
    {
        private static int[] _mechX;
        private static int[] _mechY;
        private static int _numMechs;
        private static int[] _mechTime;

        public static int CurrentUser = 255;

        public static void Initialize()
        {
            _mechX = new int[1000];
            _mechY = new int[1000];
            _mechTime = new int[1000];
        }

        public static void Unload()
        {
            _mechX = null;
            _mechY = null;
            _mechTime = null;
        }

        public static void SetCurrentUser(int plr = -1)
        {
            if (plr < 0 || plr > 255)
            {
                plr = 255;
            }
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                plr = Main.myPlayer;
            }
            CurrentUser = plr;
        }

        public static void UpdateMech()
        {
            SetCurrentUser();
            for (var i = _numMechs - 1; i >= 0; i--)
            {
                _mechTime[i]--;

                int mechX = _mechX[i];
                int mechY = _mechY[i];

                if (!WorldGen.InWorld(mechX, mechY, 1))
                {
                    _numMechs--;
                }
                else
                {
                    if (Main.tile[mechX, mechY].HasTile && Main.tile[mechX, mechY].TileType == 144)
                    {
                        if (Main.tile[mechX, mechY].TileFrameY == 0)
                        {
                            _mechTime[i] = 0;
                        }
                        else
                        {
                            var num = Main.tile[mechX, mechY].TileFrameX / 18;
                            if (num == 0)
                            {
                                num = 60;
                            }
                            else if (num == 1)
                            {
                                num = 180;
                            }
                            else if (num == 2)
                            {
                                num = 300;
                            }
                            else if (num == 3)
                            {
                                num = 30;
                            }
                            else if (num == 4)
                            {
                                num = 15;
                            }
                            if (Math.IEEERemainder(_mechTime[i], num) == 0.0)
                            {
                                _mechTime[i] = 18000;
                                Interface.InputActivate(new Point16(mechX, mechY));
                            }
                        }
                    }
                    if (_mechTime[i] <= 0)
                    {
                        if (Main.tile[mechX, mechY].HasTile && Main.tile[mechX, mechY].TileType == 144)
                        {
                            Main.tile[mechX, mechY].TileFrameY = 0;
                            NetMessage.SendTileSquare(-1, mechX, mechY, TileChangeType.None);
                        }
                        if (Main.tile[mechX, mechY].HasTile && Main.tile[mechX, mechY].TileType == 411)
                        {
                            var tile = Main.tile[mechX, mechY];
                            var num2 = tile.TileFrameX % 36 / 18;
                            var num3 = tile.TileFrameY % 36 / 18;
                            var num4 = mechX - num2;
                            var num5 = mechY - num3;
                            var num6 = 36;
                            if (Main.tile[num4, num5].TileFrameX >= 36)
                            {
                                num6 = -36;
                            }
                            for (var j = num4; j < num4 + 2; j++)
                            {
                                for (var k = num5; k < num5 + 2; k++)
                                {
                                    if (WorldGen.InWorld(j, k, 1))
                                    {
                                        Main.tile[j, k].TileFrameX = (short)(Main.tile[j, k].TileFrameX + num6);
                                    }
                                }
                            }
                            NetMessage.SendTileSquare(-1, num4, num5, 2, 2, TileChangeType.None);
                        }
                        for (var l = i; l < _numMechs; l++)
                        {
                            _mechX[l] = _mechX[l + 1];
                            _mechY[l] = _mechY[l + 1];
                            _mechTime[l] = _mechTime[l + 1];
                        }
                        _numMechs--;
                    }
                }
            }
        }

        public static bool CheckMech(int i, int j, int time)
        {
            for (int k = 0; k < _numMechs; k++)
            {
                if (_mechX[k] == i && _mechY[k] == j)
                {
                    return false;
                }
            }
            if (_numMechs < 999)
            {
                _mechX[_numMechs] = i;
                _mechY[_numMechs] = j;
                _mechTime[_numMechs] = time;
                _numMechs++;
                return true;
            }
            return false;
        }

        public static void HitSwitch(int x, int y)
        {
            var hitPos = new Point16(x, y);
            if (Input.TryGetType(Main.tile[hitPos], out var type))
            {
                Input.Activate(type, hitPos);
            }
            else
            {
                Main.NewText($"Not input at {hitPos}");
            }
        }
    }
}
