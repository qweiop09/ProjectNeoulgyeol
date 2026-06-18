using UnityEngine;
using _01_Scripts.Runtime.Worlds;

namespace _01_Scripts.Runtime.Battles
{
    public class BattleStarter : MonoBehaviour
    {
        [SerializeField] private BattleManager _battleManager;

        private void Start()
        {
            var playerParty = BattleContext.PlayerParty;
            var enemyParty = BattleContext.EnemyParty;

            if (playerParty == null || enemyParty == null)
            {
                Debug.LogWarning("[BattleStarter] BattleContext에 데이터가 없습니다. 테스트 시작 버튼을 사용하세요.");
                return;
            }

            BattleContext.Clear();
            _battleManager.BattleStart(playerParty, enemyParty);
        }
    }
}
