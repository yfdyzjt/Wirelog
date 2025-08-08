using Terraria.GameContent.Events;

namespace Wirelog.Outputs
{
    public static class PartyMonolith
    {
        public static void Activate(OutputPort outputPort)
        {
            BirthdayParty.ToggleManualParty();
        }
    }
}