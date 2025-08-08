using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace Wirelog.Outputs
{
    public static class Statues
    {
        public static void Activate(OutputPort outputPort)
        {
            var tile = Main.tile[outputPort.Output.Pos];
            if (tile.TileType == 531)
            {
                int num119 = (int)(tile.TileFrameX / 36);
                int num120 = (int)(tile.TileFrameY / 54);
                int num121 = outputPort.Output.Pos.X - ((int)tile.TileFrameX - num119 * 36) / 18;
                int num122 = outputPort.Output.Pos.Y - ((int)tile.TileFrameY - num120 * 54) / 18;
                if (WiringWrapper.CheckMech(num121, num122, 900))
                {
                    Vector2 vector2 = new Vector2((float)(num121 + 1), (float)num122) * 16f;
                    vector2.Y += 28f;
                    int num123 = 99;
                    int damage3 = 70;
                    float knockBack2 = 10f;
                    if (num123 != 0)
                    {
                        Projectile.NewProjectile(Wiring.GetProjectileSource(num121, num122), (float)((int)vector2.X), (float)((int)vector2.Y), 0f, 0f, num123, damage3, knockBack2, Main.myPlayer, 0f, 0f, 0f);
                        return;
                    }
                }
            }
            else if (tile.TileType == 349)
            {
                int num148 = (int)(tile.TileFrameY / 18);
                num148 %= 3;
                int num149 = outputPort.Output.Pos.Y - num148;
                int num150;
                for (num150 = (int)(tile.TileFrameX / 18); num150 >= 2; num150 -= 2)
                {
                }
                num150 = outputPort.Output.Pos.X - num150;
                short num151;
                if (Main.tile[num150, num149].TileFrameX == 0)
                {
                    num151 = 216;
                }
                else
                {
                    num151 = -216;
                }
                for (int num152 = 0; num152 < 2; num152++)
                {
                    for (int num153 = 0; num153 < 3; num153++)
                    {
                        Tile tile9 = Main.tile[num150 + num152, num149 + num153];
                        tile9.TileFrameX += num151;
                    }
                }
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, num150, num149, 2, 3, TileChangeType.None);
                }
                Animation.NewTemporaryAnimation((num151 > 0) ? 0 : 1, 349, num150, num149);
            }
            else if(tile.TileType == 506)
            {
                int num154 = (int)(tile.TileFrameY / 18);
                num154 %= 3;
                int num155 = outputPort.Output.Pos.Y - num154;
                int num156;
                for (num156 = (int)(tile.TileFrameX / 18); num156 >= 2; num156 -= 2)
                {
                }
                num156 = outputPort.Output.Pos.X - num156;
                short num157;
                if (Main.tile[num156, num155].TileFrameX < 72)
                {
                    num157 = 72;
                }
                else
                {
                    num157 = -72;
                }
                for (int num158 = 0; num158 < 2; num158++)
                {
                    for (int num159 = 0; num159 < 3; num159++)
                    {
                        Tile tile10 = Main.tile[num156 + num158, num155 + num159];
                        tile10.TileFrameX += num157;
                    }
                }
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, num156, num155, 2, 3, TileChangeType.None);
                    return;
                }
            }
            else if(tile.TileType == 105)
            {
                int num130 = outputPort.Output.Pos.Y - (int)(tile.TileFrameY / 18);
                int num131 = (int)(tile.TileFrameX / 18);
                int num132 = 0;
                while (num131 >= 2)
                {
                    num131 -= 2;
                    num132++;
                }
                num131 = outputPort.Output.Pos.X - num131;
                num131 = outputPort.Output.Pos.X - (int)(tile.TileFrameX % 36 / 18);
                num130 = outputPort.Output.Pos.Y - (int)(tile.TileFrameY % 54 / 18);
                int num133 = (int)(tile.TileFrameY / 54);
                num133 %= 3;
                num132 = (int)(tile.TileFrameX / 36) + num133 * 55;
                int num134 = num131 * 16 + 16;
                int num135 = (num130 + 3) * 16;
                int num136 = -1;
                int num137 = -1;
                bool flag11 = true;
                bool flag12 = false;
                if (num132 != 5)
                {
                    if (num132 != 13)
                    {
                        switch (num132)
                        {
                            case 30:
                                num137 = 6;
                                break;
                            case 35:
                                num137 = 2;
                                break;
                            case 51:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            299,
                                                                            538
                                });
                                break;
                            case 52:
                                num137 = 356;
                                break;
                            case 53:
                                num137 = 357;
                                break;
                            case 54:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            355,
                                                                            358
                                });
                                break;
                            case 55:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            367,
                                                                            366
                                });
                                break;
                            case 56:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            359,
                                                                            359,
                                                                            359,
                                                                            359,
                                                                            360
                                });
                                break;
                            case 57:
                                num137 = 377;
                                break;
                            case 58:
                                num137 = 300;
                                break;
                            case 59:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            364,
                                                                            362
                                });
                                break;
                            case 60:
                                num137 = 148;
                                break;
                            case 61:
                                num137 = 361;
                                break;
                            case 62:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            487,
                                                                            486,
                                                                            485
                                });
                                break;
                            case 63:
                                num137 = 164;
                                flag11 &= NPC.MechSpawn((float)num134, (float)num135, 165);
                                break;
                            case 64:
                                num137 = 86;
                                flag12 = true;
                                break;
                            case 65:
                                num137 = 490;
                                break;
                            case 66:
                                num137 = 82;
                                break;
                            case 67:
                                num137 = 449;
                                break;
                            case 68:
                                num137 = 167;
                                break;
                            case 69:
                                num137 = 480;
                                break;
                            case 70:
                                num137 = 48;
                                break;
                            case 71:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            170,
                                                                            180,
                                                                            171
                                });
                                flag12 = true;
                                break;
                            case 72:
                                num137 = 481;
                                break;
                            case 73:
                                num137 = 482;
                                break;
                            case 74:
                                num137 = 430;
                                break;
                            case 75:
                                num137 = 489;
                                break;
                            case 76:
                                num137 = 611;
                                break;
                            case 77:
                                num137 = 602;
                                break;
                            case 78:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            595,
                                                                            596,
                                                                            599,
                                                                            597,
                                                                            600,
                                                                            598
                                });
                                break;
                            case 79:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            616,
                                                                            617
                                });
                                break;
                            case 80:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            671,
                                                                            672
                                });
                                break;
                            case 81:
                                num137 = 673;
                                break;
                            case 82:
                                num137 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                {
                                                                            674,
                                                                            675
                                });
                                break;
                        }
                    }
                    else
                    {
                        num137 = 24;
                    }
                }
                else
                {
                    num137 = 73;
                }
                if (num137 != -1 && WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, num137) && flag11)
                {
                    if (!flag12 || !Collision.SolidTiles(num131 - 2, num131 + 3, num130, num130 + 2))
                    {
                        num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135, num137, 0, 0f, 0f, 0f, 0f, 255);
                    }
                    else
                    {
                        Vector2 vector3 = new Vector2((float)(num134 - 4), (float)(num135 - 22)) - new Vector2(10f);
                        Utils.PoofOfSmoke(vector3);
                        NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)vector3.X, vector3.Y, 0f, 0f, 0, 0, 0);
                    }
                }
                if (num136 <= -1)
                {
                    if (num132 == 4)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 1))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, 1, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 7)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 49))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134 - 4, num135 - 6, 49, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 8)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 55))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, 55, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 9)
                    {
                        int type4 = 46;
                        if (BirthdayParty.PartyIsUp)
                        {
                            type4 = 540;
                        }
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, type4))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, type4, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 10)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 21))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135, 21, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 16)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 42))
                        {
                            if (!Collision.SolidTiles(num131 - 1, num131 + 1, num130, num130 + 1))
                            {
                                num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, 42, 0, 0f, 0f, 0f, 0f, 255);
                            }
                            else
                            {
                                Vector2 vector4 = new Vector2((float)(num134 - 4), (float)(num135 - 22)) - new Vector2(10f);
                                Utils.PoofOfSmoke(vector4);
                                NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)vector4.X, vector4.Y, 0f, 0f, 0, 0, 0);
                            }
                        }
                    }
                    else if (num132 == 18)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 67))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, 67, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 23)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 63))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, 63, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 27)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 85))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134 - 9, num135, 85, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 28)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 74))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, (int)Utils.SelectRandom<short>(Main.rand, new short[]
                            {
                                                                            74,
                                                                            297,
                                                                            298
                            }), 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 34)
                    {
                        for (int num138 = 0; num138 < 2; num138++)
                        {
                            for (int num139 = 0; num139 < 3; num139++)
                            {
                                Tile tile8 = Main.tile[num131 + num138, num130 + num139];
                                tile8.TileType = 349;
                                tile8.TileFrameX = (short)(num138 * 18 + 216);
                                tile8.TileFrameY = (short)(num139 * 18);
                            }
                        }
                        Animation.NewTemporaryAnimation(0, 349, num131, num130);
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendTileSquare(-1, num131, num130, 2, 3, TileChangeType.None);
                        }
                    }
                    else if (num132 == 42)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 58))
                        {
                            num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, 58, 0, 0f, 0f, 0f, 0f, 255);
                        }
                    }
                    else if (num132 == 37)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 600) && Item.MechSpawn((float)num134, (float)num135, 58) && Item.MechSpawn((float)num134, (float)num135, 1734) && Item.MechSpawn((float)num134, (float)num135, 1867))
                        {
                            Item.NewItem(Wiring.GetItemSource(num134, num135), num134, num135 - 16, 0, 0, 58, 1, false, 0, false, false);
                        }
                    }
                    else if (num132 == 50)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 30) && NPC.MechSpawn((float)num134, (float)num135, 65))
                        {
                            if (!Collision.SolidTiles(num131 - 2, num131 + 3, num130, num130 + 2))
                            {
                                num136 = NPC.NewNPC(Wiring.GetNPCSource(num131, num130), num134, num135 - 12, 65, 0, 0f, 0f, 0f, 0f, 255);
                            }
                            else
                            {
                                Vector2 vector5 = new Vector2((float)(num134 - 4), (float)(num135 - 22)) - new Vector2(10f);
                                Utils.PoofOfSmoke(vector5);
                                NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)vector5.X, vector5.Y, 0f, 0f, 0, 0, 0);
                            }
                        }
                    }
                    else if (num132 == 2)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 600) && Item.MechSpawn((float)num134, (float)num135, 184) && Item.MechSpawn((float)num134, (float)num135, 1735) && Item.MechSpawn((float)num134, (float)num135, 1868))
                        {
                            Item.NewItem(Wiring.GetItemSource(num134, num135), num134, num135 - 16, 0, 0, 184, 1, false, 0, false, false);
                        }
                    }
                    else if (num132 == 17)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 600) && Item.MechSpawn((float)num134, (float)num135, 166))
                        {
                            Item.NewItem(Wiring.GetItemSource(num134, num135), num134, num135 - 20, 0, 0, 166, 1, false, 0, false, false);
                        }
                    }
                    else if (num132 == 40)
                    {
                        if (WiringWrapper.CheckMech(num131, num130, 300))
                        {
                            int num140 = 50;
                            int[] array = new int[num140];
                            int num141 = 0;
                            for (int num142 = 0; num142 < 200; num142++)
                            {
                                if (Main.npc[num142].active && (Main.npc[num142].type == NPCID.Merchant || Main.npc[num142].type == NPCID.ArmsDealer || Main.npc[num142].type == NPCID.Guide || Main.npc[num142].type == NPCID.Demolitionist || Main.npc[num142].type == NPCID.Clothier || Main.npc[num142].type == NPCID.GoblinTinkerer || Main.npc[num142].type == NPCID.Wizard || Main.npc[num142].type == NPCID.SantaClaus || Main.npc[num142].type == NPCID.Truffle || Main.npc[num142].type == NPCID.DyeTrader || Main.npc[num142].type == NPCID.Cyborg || Main.npc[num142].type == NPCID.Painter || Main.npc[num142].type == NPCID.WitchDoctor || Main.npc[num142].type == NPCID.Pirate || Main.npc[num142].type == NPCID.TravellingMerchant || Main.npc[num142].type == NPCID.Angler || Main.npc[num142].type == NPCID.DD2Bartender || Main.npc[num142].type == NPCID.TaxCollector || Main.npc[num142].type == NPCID.Golfer))
                                {
                                    array[num141] = num142;
                                    num141++;
                                    if (num141 >= num140)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (num141 > 0)
                            {
                                int num143 = array[Main.rand.Next(num141)];
                                Main.npc[num143].position.X = (float)(num134 - Main.npc[num143].width / 2);
                                Main.npc[num143].position.Y = (float)(num135 - Main.npc[num143].height - 1);
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num143, 0f, 0f, 0f, 0, 0, 0);
                            }
                        }
                    }
                    else if (num132 == 41 && WiringWrapper.CheckMech(num131, num130, 300))
                    {
                        int num144 = 50;
                        int[] array2 = new int[num144];
                        int num145 = 0;
                        for (int num146 = 0; num146 < 200; num146++)
                        {
                            if (Main.npc[num146].active && (Main.npc[num146].type == NPCID.Nurse || Main.npc[num146].type == NPCID.Dryad || Main.npc[num146].type == NPCID.Mechanic || Main.npc[num146].type == NPCID.Steampunker || Main.npc[num146].type == NPCID.PartyGirl || Main.npc[num146].type == NPCID.Stylist || Main.npc[num146].type == NPCID.BestiaryGirl || Main.npc[num146].type == NPCID.Princess))
                            {
                                array2[num145] = num146;
                                num145++;
                                if (num145 >= num144)
                                {
                                    break;
                                }
                            }
                        }
                        if (num145 > 0)
                        {
                            int num147 = array2[Main.rand.Next(num145)];
                            Main.npc[num147].position.X = (float)(num134 - Main.npc[num147].width / 2);
                            Main.npc[num147].position.Y = (float)(num135 - Main.npc[num147].height - 1);
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num147, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                }
                if (num136 >= 0)
                {
                    Main.npc[num136].value = 0f;
                    Main.npc[num136].npcSlots = 0f;
                    Main.npc[num136].SpawnedFromStatue = true;
                    Main.npc[num136].CanBeReplacedByOtherNPCs = true;
                    return;
                }
            }
        }
    }
}