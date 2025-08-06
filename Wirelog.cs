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
            On_WorldGen.SaveAndQuit += WorldGen_SaveAndQuit;
            On_Wiring.Initialize += Wiring_Initialize;
            On_Wiring.UpdateMech += Wiring_UpdateMech;
            On_Wiring.CheckMech += Wiring_CheckMech;
            On_Wiring.HitSwitch += Wiring_HitSwitch;
        }

        private void WorldGen_SaveAndQuit(On_WorldGen.orig_SaveAndQuit orig, Action callback)
        {
            VerilogSimulator.Stop();
            orig(callback);
        }

        private void Wiring_Initialize(On_Wiring.orig_Initialize orig)
        {
            WiringWrapper.Initialize();
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
            On_WorldGen.SaveAndQuit -= WorldGen_SaveAndQuit;
            On_Wiring.Initialize -= Wiring_Initialize;
            On_Wiring.UpdateMech -= Wiring_UpdateMech;
            On_Wiring.CheckMech -= Wiring_CheckMech;
            On_Wiring.HitSwitch -= Wiring_HitSwitch;
        }
    }
}
