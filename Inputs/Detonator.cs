using Terraria.Audio;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using System;

namespace Wirelog.Inputs
{
    public static class Detonator
    {
        public static void Activate(Point16 pos) => Lever.Activate(pos);
    }
}