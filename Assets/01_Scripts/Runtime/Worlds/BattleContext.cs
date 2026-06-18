namespace _01_Scripts.Runtime.Worlds
{
    public static class BattleContext
    {
        public static EncounterResult PendingEncounter { get; private set; }

        public static void Set(EncounterResult result)
        {
            PendingEncounter = result;
        }

        public static void Clear()
        {
            PendingEncounter = null;
        }
    }
}
