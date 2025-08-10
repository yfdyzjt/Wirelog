using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Wirelog
{
    internal class WiringWrapper
    {
        private static readonly int[] _mechX = new int[1000];
        private static readonly int[] _mechY = new int[1000];
        private static readonly int[] _mechTime = new int[1000];
        private static int _numMechs;

        public static int NumInPump => InPump.Count;
        public static int NumOutPump => OutPump.Count;
        public static HashSet<Point16> InPump { get; } = [];
        public static HashSet<Point16> OutPump { get; } = [];
        public static Vector2[] TeleportPos { get; } = [Vector2.One * -1f, Vector2.One * -1f];
        public static int CurrentUser { get; set; } = 255;

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

        public static void XferWater()
        {
            foreach (var inPump in InPump)
            {
                var inPumpX = inPump.X;
                var inPumpY = inPump.Y;
                var inPumpTile = Main.tile[inPumpX, inPumpY];
                if (inPumpTile.LiquidAmount > 0)
                {
                    var inPumpLiquidType = Main.tile[inPumpX, inPumpY].LiquidType;
                    foreach (var outPump in OutPump)
                    {
                        var outPumpX = outPump.X;
                        var outPumpY = outPump.Y;
                        var outPumpTile = Main.tile[outPumpX, outPumpY];
                        if (outPumpTile.LiquidAmount < 255)
                        {
                            var outPumpLiquidType = Main.tile[outPumpX, outPumpY].LiquidType;
                            if (outPumpTile.LiquidAmount == 0)
                            {
                                outPumpLiquidType = inPumpLiquidType;
                            }
                            if (outPumpLiquidType == inPumpLiquidType)
                            {
                                var moveLiquidAmount = inPumpTile.LiquidAmount;
                                if (moveLiquidAmount + outPumpTile.LiquidAmount > 255)
                                {
                                    moveLiquidAmount = (byte)(255 - outPumpTile.LiquidAmount);
                                }

                                outPumpTile.LiquidAmount += moveLiquidAmount;
                                inPumpTile.LiquidAmount -= moveLiquidAmount;

                                outPumpTile.LiquidType = inPumpLiquidType;

                                WorldGen.SquareTileFrame(outPumpX, outPumpY, true);
                                if (inPumpTile.LiquidAmount == 0)
                                {
                                    inPumpTile.LiquidType = LiquidID.Water;
                                    WorldGen.SquareTileFrame(inPumpX, inPumpY, true);
                                    break;
                                }
                            }
                        }
                    }
                    WorldGen.SquareTileFrame(inPumpX, inPumpY, true);
                }
            }
        }

        public static void Teleport()
        {
            if (TeleportPos[0].X < TeleportPos[1].X + 3f && TeleportPos[0].X > TeleportPos[1].X - 3f && TeleportPos[0].Y > TeleportPos[1].Y - 3f && TeleportPos[0].Y < TeleportPos[1].Y)
            {
                return;
            }
            Rectangle[] array = new Rectangle[2];
            array[0].X = (int)(TeleportPos[0].X * 16f);
            array[0].Width = 48;
            array[0].Height = 48;
            array[0].Y = (int)(TeleportPos[0].Y * 16f - array[0].Height);
            array[1].X = (int)(TeleportPos[1].X * 16f);
            array[1].Width = 48;
            array[1].Height = 48;
            array[1].Y = (int)(TeleportPos[1].Y * 16f - array[1].Height);
            for (int i = 0; i < 2; i++)
            {
                Vector2 value = new(array[1].X - array[0].X, array[1].Y - array[0].Y);
                if (i == 1)
                {
                    value = new Vector2(array[0].X - array[1].X, array[0].Y - array[1].Y);
                }
                if (!Wiring.blockPlayerTeleportationForOneIteration)
                {
                    for (int j = 0; j < 255; j++)
                    {
                        if (Main.player[j].active && !Main.player[j].dead && !Main.player[j].teleporting && TeleporterHitboxIntersects(array[i], Main.player[j].Hitbox))
                        {
                            Vector2 vector = Main.player[j].position + value;
                            Main.player[j].teleporting = true;
                            if (Main.netMode == 2)
                            {
                                RemoteClient.CheckSection(j, vector, 1);
                            }
                            Main.player[j].Teleport(vector, 0, 0);
                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendData(65, -1, -1, null, 0, j, vector.X, vector.Y, 0, 0, 0);
                            }
                        }
                    }
                }
                for (int k = 0; k < 200; k++)
                {
                    if (Main.npc[k].active && !Main.npc[k].teleporting && Main.npc[k].lifeMax > 5 && !Main.npc[k].boss && !Main.npc[k].noTileCollide)
                    {
                        int type = Main.npc[k].type;
                        if (!NPCID.Sets.TeleportationImmune[type] && TeleporterHitboxIntersects(array[i], Main.npc[k].Hitbox))
                        {
                            Main.npc[k].teleporting = true;
                            Main.npc[k].Teleport(Main.npc[k].position + value, 0, 0);
                        }
                    }
                }
            }
            for (int l = 0; l < 255; l++)
            {
                Main.player[l].teleporting = false;
            }
            for (int m = 0; m < 200; m++)
            {
                Main.npc[m].teleporting = false;
            }
        }

        public static bool TeleporterHitboxIntersects(Rectangle teleporter, Rectangle entity)
        {
            Rectangle rectangle = Rectangle.Union(teleporter, entity);
            return rectangle.Width <= teleporter.Width + entity.Width && rectangle.Height <= teleporter.Height + entity.Height;
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
