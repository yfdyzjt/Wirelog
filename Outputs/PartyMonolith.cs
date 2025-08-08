using Terraria.DataStructures;
using Terraria.GameContent.Events;

namespace Wirelog.Outputs
{
    public static class PartyMonolith
    {
        public static void Activate(Point16 pos)
        {
            BirthdayParty.ToggleManualParty();
        }
    }
}