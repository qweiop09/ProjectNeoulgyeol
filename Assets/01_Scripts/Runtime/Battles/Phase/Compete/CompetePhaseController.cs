using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Worlds;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class CompetePhaseController : MonoBehaviour
{
    // class Variables
    [SerializeField] private BattleManager battleManager; 
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    [SerializeField] private CompeteContestController competeContestController;

    [Header("도망")]
    [SerializeField] private float battleEscapeDifficulty = 1f;
    [SerializeField] private float minEscapeChance = 0.05f;
    [SerializeField] private float maxEscapeChance = 0.95f;

    // data Variables
    private CharacterHandler[] allCharacterHandlers;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnCompetePhaseStart   += StartCompetePhaseStartProcess;
        battlePhaseCoordinator.OnCompetePhasePerform += StartCompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd     += StartCompetePhaseEndProcess;
    }

    private void OnDestroy()
    {
        battlePhaseCoordinator.OnCompetePhaseStart   -= StartCompetePhaseStartProcess;
        battlePhaseCoordinator.OnCompetePhasePerform -= StartCompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd     -= StartCompetePhaseEndProcess;
    }
    
    // Start Phase Actions
    private void StartCompetePhaseStartProcess(CharacterHandler[] _allCharacterHandlers)
    {
        allCharacterHandlers = null;
        allCharacterHandlers = _allCharacterHandlers;

        // 도망 판정 — 경합 페이즈가 시작하는 이 시점에 그룹 전체를 한 번에 판정한다.
        // 성공하면 전투가 즉시 끝나므로 평소 진행(CompleteCompetePhaseStartProcess)은 하지 않는다.
        if (TryResolveEscape(allCharacterHandlers)) return;

        CompleteCompetePhaseStartProcess();
    }

    // 도망을 선택한 아군이 있으면 그룹 단위로 도망 확률을 계산해서 판정한다.
    // 성공 시 true를 반환하며, 그 안에서 전투 종료까지 처리한다.
    private bool TryResolveEscape(CharacterHandler[] characters)
    {
        // 정상 경로면 CharactorActionMenuHandler가 선택 단계에서 이미 막아주지만,
        // 혹시 모를 우회 경로에 대비해 실제 판정 직전에도 한 번 더 확인한다.
        if (BattleContext.CurrentRoomData != null && !BattleContext.CurrentRoomData.canEscape) return false;

        List<CharacterHandler> runningAllies = characters.Where(c =>
            c.characterType == CharacterHandler.CharacterType.Friendly
            && c.GetCharacterStatus().currentState != CharacterState.Dead
            && c.TargetingData != null
            && c.TargetingData.Any(a => a is RunActData)).ToList();

        if (runningAllies.Count == 0) return false;

        float allySpeedSum = runningAllies.Sum(c => c.CurrentSpeed);

        var enemies = characters.Where(c =>
                c.characterType == CharacterHandler.CharacterType.Enemy
                && c.GetCharacterStatus().currentState != CharacterState.Dead)
            .Select(c => ((float)c.CurrentSpeed, c.GetCharacterStatus().CharacterData.escapeResistance));

        float chance = EscapeCalculation.CalculateChance(
            allySpeedSum, enemies, battleEscapeDifficulty, minEscapeChance, maxEscapeChance);
        bool success = Random.value < chance;

        Debug.Log($"[CompetePhaseController] 도망 판정: {runningAllies.Count}명 시도, 확률 {chance:P0}, 결과 {(success ? "성공" : "실패")}");

        if (!success) return false;

        CharacterHandler[] friendlyCharacters = characters
            .Where(c => c.characterType == CharacterHandler.CharacterType.Friendly).ToArray();
        battlePhaseCoordinator.CompleteBattleEnd(friendlyCharacters, BattleOutcome.Retreat);
        return true;
    }

    private void CompleteCompetePhaseStartProcess()
    {
        Debug.Log("Compete Phase Started");
        battlePhaseCoordinator.CompleteCompeteStart();
    }
    
    // Middle Phase Actions
    private async void StartCompetePhaseMiddleProcess()
    {
        // 모든 캐릭터의 행동을 실행
        for (int i = 0; i < allCharacterHandlers.Count(); i++)
        {
            CharacterHandler currentCharacter = allCharacterHandlers[i];

            // Compete Cycle Phase
            // 한 캐릭터의 모든 행동을 실행
            await competeContestController.StartCompeteCycle(currentCharacter.TargetingData);
            Debug.Log("Compete Cycle Completed for Character: " + currentCharacter.name);
        }

        CompleteCompetePhaseMiddleProcess();
    }
    
    private void CompleteCompetePhaseMiddleProcess()
    {
        Debug.Log("Compete Phase Performing");
        battlePhaseCoordinator.CompleteCompetePerform();
    }
    
    // End Phase Actions
    private void StartCompetePhaseEndProcess()
    {
        CompleteCompetePhaseEndProcess();
    }
    
    private void CompleteCompetePhaseEndProcess()
    {
        Debug.Log("Compete Phase Ended");

        var characters = allCharacterHandlers;
        allCharacterHandlers = null;

        battlePhaseCoordinator.CompleteCompeteEnd(characters);
    }
}
}
