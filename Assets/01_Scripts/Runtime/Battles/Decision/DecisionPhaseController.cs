using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class DecisionPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    private CharacterStatus[] playerCharacterTargetingDatas;
    private CharacterStatus[] enemyCharacterTargetingDatas;
    
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
        
        // 속도 설정 (배열 정렬까지)
        
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
        // CompleteDecisionPhaseMiddleProcess();
        return;
    }
    
    private void CompleteDecisionPhaseMiddleProcess()
    {
        Debug.Log("Decision Phase Performing");
        
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
