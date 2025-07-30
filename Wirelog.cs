using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.IO;
using Terraria.ModLoader;

namespace Wirelog
{
	public class Wirelog : Mod
    {
        public override void Load()
        {
            WorldFile.OnWorldLoad += Converter.Convert;
            // Terraria.On_Wiring.Initialize += Wiring_Initialize;
            // Terraria.On_Wiring.UpdateMech += Wiring_UpdateMech;
            // Terraria.On_Wiring.HitSwitch += Wiring_HitSwitch;
        }

        private void Wiring_Initialize(Terraria.On_Wiring.orig_Initialize orig)
        {
            WiringWrapper.Initialize();
        }

        private void Wiring_UpdateMech(Terraria.On_Wiring.orig_UpdateMech orig)
        {
            WiringWrapper.UpdateMech();
        }

        private void Wiring_HitSwitch(Terraria.On_Wiring.orig_HitSwitch orig, int i, int j)
        {
            WiringWrapper.HitSwitch(i, j);
        }

        public override void Unload()
        {
            WorldFile.OnWorldLoad -= Converter.Convert;
            // Terraria.On_Wiring.Initialize -= Wiring_Initialize;
            // Terraria.On_Wiring.UpdateMech -= Wiring_UpdateMech;
            // Terraria.On_Wiring.HitSwitch -= Wiring_HitSwitch;
        }
    }
}
