using System;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace Wirelog
{
    public class Wirelog : Mod
    {
        public override void Load()
        {
            WorldFile.OnWorldLoad += Converter.Convert;
            WorldFile.OnWorldLoad += VerilogSimulator.Start;
            On_WorldGen.UpdateWorld += WorldGen_UpdateWorld;
            On_WorldGen.SaveAndQuit += WorldGen_SaveAndQuit;
            On_Wiring.SetCurrentUser += Wiring_SetCurrentUser;
            On_Wiring.XferWater += Wiring_XferWater;
            On_Wiring.Teleport += Wiring_Teleport;
            On_Wiring.TeleporterHitboxIntersects += Wiring_TeleporterHitboxIntersects;
            On_Wiring.UpdateMech += Wiring_UpdateMech;
            On_Wiring.CheckMech += Wiring_CheckMech;
            On_Wiring.HitSwitch += Wiring_HitSwitch;
        }

        private void WorldGen_SaveAndQuit(On_WorldGen.orig_SaveAndQuit orig, Action callback)
        {
            VerilogSimulator.Stop();
            orig(callback);
        }

        private void WorldGen_UpdateWorld(On_WorldGen.orig_UpdateWorld orig)
        {
            orig();
            VerilogSimulator.FrameSync();
            Interface.OutputsActivate();
        }

        private static void Wiring_SetCurrentUser(On_Wiring.orig_SetCurrentUser orig, int plr)
        {
            WiringWrapper.SetCurrentUser(plr);
        }

        private void Wiring_XferWater(On_Wiring.orig_XferWater orig)
        {
            WiringWrapper.XferWater();
        }

        private void Wiring_Teleport(On_Wiring.orig_Teleport orig)
        {
            WiringWrapper.Teleport();
        }

        private bool Wiring_TeleporterHitboxIntersects(On_Wiring.orig_TeleporterHitboxIntersects orig, Microsoft.Xna.Framework.Rectangle teleporter, Microsoft.Xna.Framework.Rectangle entity)
        {
            return WiringWrapper.TeleporterHitboxIntersects(teleporter, entity);
        }

        private void Wiring_UpdateMech(On_Wiring.orig_UpdateMech orig)
        {
            WiringWrapper.UpdateMech();
        }

        private bool Wiring_CheckMech(On_Wiring.orig_CheckMech orig, int i, int j, int time)
        {
            return WiringWrapper.CheckMech(i, j, time);
        }

        private void Wiring_HitSwitch(On_Wiring.orig_HitSwitch orig, int i, int j)
        {
            WiringWrapper.HitSwitch(i, j);
        }

        public override void Unload()
        {
            WorldFile.OnWorldLoad -= Converter.Convert;
            WorldFile.OnWorldLoad -= VerilogSimulator.Start;
            On_WorldGen.UpdateWorld -= WorldGen_UpdateWorld;
            On_WorldGen.SaveAndQuit -= WorldGen_SaveAndQuit;
            On_Wiring.SetCurrentUser -= Wiring_SetCurrentUser;
            On_Wiring.XferWater -= Wiring_XferWater;
            On_Wiring.Teleport -= Wiring_Teleport;
            On_Wiring.TeleporterHitboxIntersects -= Wiring_TeleporterHitboxIntersects;
            On_Wiring.UpdateMech -= Wiring_UpdateMech;
            On_Wiring.CheckMech -= Wiring_CheckMech;
            On_Wiring.HitSwitch -= Wiring_HitSwitch;
        }
    }
}
