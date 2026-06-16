using System.Collections.Generic;
using System.Threading.Tasks;
using _01_Scripts.DTO;
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
    
    private void StartClosePhase(CharacterHandler[] allCharacterHandlers )
    {
        allCharacterHandlers = allCharacterHandlers;
        
        CompleteClosePhaseProcess(allCharacterHandlers);
    }
    
    private async void CompleteClosePhaseProcess(CharacterHandler[] allCharacterHandlers)
    {
        // Close Phase Logic
    
        for(int i = 0; i < allCharacterHandlers.Length; i++)
        {
            Debug.Log("Character " + i + ": " + allCharacterHandlers[i].name);
            allCharacterHandlers[i].GetCharacterBattleData().TargetingData
                = new ActData[allCharacterHandlers[i].GetCharacterBattleData().TargetingData.Length];
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
        
            battlePhaseCoordinator.CompleteBattleEnd(friendlyCharacters.ToArray(), isEnemyAllDead);
        }
    
        Debug.Log("Close Phase Ended");
        await Wait(0.4f);

        await CameraHandler.Instance.PositionResetToLerp();
    
        // Close Phase End
        battlePhaseCoordinator.CompleteClosePhaseEnd(
            friendlyCharacters.ToArray(), enemyCharacters.ToArray());
    }

    // 해당 진영 전원 사망 여부 체크
    private bool IsAllDead(List<CharacterHandler> characters)
    {
        if (characters.Count == 0) return false;

        foreach (CharacterHandler character in characters)
        {
            if (character.GetCharacterBattleData().currentState != CharacterState.Dead)
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
