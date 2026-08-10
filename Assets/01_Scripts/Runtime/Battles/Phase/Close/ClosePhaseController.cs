using System.Collections.Generic;
using System.Threading.Tasks;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.DTO.Item.Effects;
using _01_Scripts.Runtime.Battles.CameraControlle;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Close
{
public class ClosePhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    private CharacterHandler[] allCharacterHandlers;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnClosePhaseEnd += StartClosePhase;
    }

    private void OnDestroy()
    {
        battlePhaseCoordinator.OnClosePhaseEnd -= StartClosePhase;
    }
    
    private void StartClosePhase(CharacterHandler[] characters)
    {
        allCharacterHandlers = null;
        allCharacterHandlers = characters;

        CompleteClosePhaseProcess(allCharacterHandlers);
    }
    
    private async void CompleteClosePhaseProcess(CharacterHandler[] allCharacterHandlers)
    {
        // Close Phase Logic
    
        for(int i = 0; i < allCharacterHandlers.Length; i++)
        {
            Debug.Log("Character " + i + ": " + allCharacterHandlers[i].name);
            allCharacterHandlers[i].TargetingData
                = new ActData[allCharacterHandlers[i].TargetingData.Length];

            DecrementBuffs(allCharacterHandlers[i].GetCharacterStatus());
        }
    
        // 아군/적군으로 분리
        List<CharacterHandler> friendlyCharacters = new List<CharacterHandler>();
        List<CharacterHandler> enemyCharacters = new List<CharacterHandler>();
    
        foreach (CharacterHandler character in allCharacterHandlers)
        {
            if (character.characterType == CharacterHandler.CharacterType.Friendly)
                friendlyCharacters.Add(character);
            else
                enemyCharacters.Add(character);
        }
    
        // 각 진영 전원 사망 여부 체크
        bool isFriendlyAllDead = IsAllDead(friendlyCharacters);
        bool isEnemyAllDead = IsAllDead(enemyCharacters);
    
        if (isFriendlyAllDead || isEnemyAllDead)
        {
            Debug.Log("전투 종료 — 아군 전멸: " + isFriendlyAllDead + " / 적군 전멸: " + isEnemyAllDead);

            BattleOutcome outcome = isEnemyAllDead ? BattleOutcome.Victory : BattleOutcome.Defeat;
            battlePhaseCoordinator.CompleteBattleEnd(friendlyCharacters.ToArray(), outcome);
        }

        Debug.Log("Close Phase Ended");
        await Wait(0.4f);

        await CameraHandler.Instance.PositionResetToLerp();

        // 전투가 끝났으면 여기서 멈춰야 한다 — 카메라 리셋 등 화면 정리는 그대로 하되, 다음 라운드(Open Phase)를
        // 여는 CompleteClosePhaseEnd는 부르면 안 된다. BattleManager.BattleEnd가 비동기로 캐릭터를 파괴하는 중이라
        // OpenPhaseController가 이미 파괴된 CharacterHandler를 만지다가 MissingReferenceException이 난다.
        if (isFriendlyAllDead || isEnemyAllDead) return;

        // Close Phase End
        var friendly = friendlyCharacters.ToArray();
        var enemy = enemyCharacters.ToArray();
        allCharacterHandlers = null;

        battlePhaseCoordinator.CompleteClosePhaseEnd(friendly, enemy);
    }

    // 라운드 종료 시 활성 버프의 남은 라운드를 감소시키고, 0이 되면 제거한다.
    // 최대치(MaxHp 등) 버프가 제거되는 경우 그 순간의 최대치를 기억해뒀다가 현재치를 클램프한다.
    private void DecrementBuffs(CharacterStatus status)
    {
        for (int i = status.activeBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = status.activeBuffs[i];
            buff.RemainingRounds--;

            if (buff.RemainingRounds > 0) continue;

            Debug.Log($"[ClosePhaseController] '{buff.Source.buffName}' 만료 ({status.CharacterData.characterName})");

            if (buff.Source is StatBuffEffect statBuff)
            {
                int previousMax = statBuff.statType switch
                {
                    BuffStatType.MaxHp      => status.GetMaxHp(),
                    BuffStatType.MaxMp      => status.GetMaxMp(),
                    BuffStatType.MaxStamina => status.GetMaxStamina(),
                    _ => -1
                };

                CharacterStatusCalculator.Instance.ExpireBuff(status, buff);

                switch (statBuff.statType)
                {
                    case BuffStatType.MaxHp:      status.OnMaxHpChanged(previousMax, false); break;
                    case BuffStatType.MaxMp:      status.OnMaxMpChanged(previousMax, false); break;
                    case BuffStatType.MaxStamina: status.OnMaxStaminaChanged(previousMax, false); break;
                }

                // OnMaxXChanged가 currentHp/Mp/Stamina를 새 최대치로 클램프할 수 있는데, 이 변화는 onStatusChanged를
                // 거치지 않아서 체력바가 갱신 안 되고 있었다 — 여기서 직접 발행해 UI를 맞춰준다.
                CharacterStatusCalculator.Instance.onStatusChanged?.Invoke(status);
            }
            else
            {
                CharacterStatusCalculator.Instance.ExpireBuff(status, buff);
            }
        }
    }

    // 해당 진영 전원 사망 여부 체크
    private bool IsAllDead(List<CharacterHandler> characters)
    {
        if (characters.Count == 0) return false;

        foreach (CharacterHandler character in characters)
        {
            if (character.GetCharacterStatus().currentState != CharacterState.Dead)
                return false;
        }

        return true;
    }
    
    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }
    
    
}
}
