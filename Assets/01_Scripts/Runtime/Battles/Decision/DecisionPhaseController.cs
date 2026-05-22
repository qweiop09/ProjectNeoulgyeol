using _01_Scripts.DTO;
using UnityEngine;
using UnityEngine.Serialization;

namespace _01_Scripts.Runtime.Battles.Decision
{
public class DecisionPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private ActionSelectionPhaseManager actionSelectionPhaseManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    private CharacterBattleData[] playerCharacterTargetingDatas;
    private CharacterBattleData[] enemyCharacterTargetingDatas;
    
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnDecisionPhaseStart += StartDecisionPhase;
        battlePhaseCoordinator.OnDecisionPhasePerform += StartDecisionPhaseMiddleProcess;
        battlePhaseCoordinator.OnDecisionPhaseEnd += StartDecisionPhaseEndProcess;
    }
    
    // Start Phase Actions
    private void StartDecisionPhase()
    {
        playerCharacterTargetingDatas = battleManager.GetPlayerCharacters();
        enemyCharacterTargetingDatas = battleManager.GetEnemyCharacters();
        
        // 속도 초기화 (배열 정렬까지)
        
        CompleteDecisionPhaseStartProcess();
    }
    
    private void CompleteDecisionPhaseStartProcess()
    {
        Debug.Log("Decision Phase Started");
        battlePhaseCoordinator.CompleteDecisionStart();
    }
    
    
    // Middle Phase Actions
    private void StartDecisionPhaseMiddleProcess()
    {
        actionSelectionPhaseManager.ActivateCharacterSelectionPhase();
        
        
        return;
    }
    
    private void CompleteDecisionPhaseMiddleProcess()
    {
        Debug.Log("Decision Phase Performing");
        actionSelectionPhaseManager.DeactivateCharacterSelectionPhase();
        
        // 타겟팅 데이터 설정
        
        battlePhaseCoordinator.CompleteDecisionPerform();
    }
    
    public void PressedCompetePhaseStartButton()
    {
        CompleteDecisionPhaseMiddleProcess();
    }
    
    
    // End Phase Actions
    private void StartDecisionPhaseEndProcess()
    {
        CompleteDecisionPhaseEndProcess();
    }
    
    private void CompleteDecisionPhaseEndProcess()
    {
        Debug.Log("Decision Phase Ended");
        
        battlePhaseCoordinator.CompleteDecisionEnd(playerCharacterTargetingDatas, enemyCharacterTargetingDatas);
    }
    
}
}
