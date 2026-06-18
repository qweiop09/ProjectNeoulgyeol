using _01_Scripts.DTO;

namespace _01_Scripts.Runtime.Worlds
{
    public static class BattleContext
    {
        public static CharacterBattleData[] PlayerParty { get; private set; }
        public static CharacterBattleData[] EnemyParty { get; private set; }

        public static void Set(CharacterBattleData[] playerParty, CharacterBattleData[] enemyParty)
        {
            PlayerParty = playerParty;
            EnemyParty = enemyParty;
        }

        public static void Clear()
        {
            PlayerParty = null;
            EnemyParty = null;
        }
    }
}
