using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CompetePhaseController : MonoBehaviour
{
    // class Variables
    [SerializeField] private BattleManager battleManager; 
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    [SerializeField] private CompeteHandler competeHandler;
    [SerializeField] private CompeteMoveHandler competeMoveHandler;
    [SerializeField] private CompeteJudgmentHandler competeJudgmentHandler;

    
    // data Variables
    private Character[] playerCharacters;
    private Character[] enemyCharacters;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        
        battlePhaseCoordinator.OnCompetePhaseStart += CompetePhaseStartProcess;
        battlePhaseCoordinator.OnCompetePhasePerform += CompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd += CompetePhaseEndProcess;
    }
    
    private void CompetePhaseStartProcess()
    {
        // Compete Phase Logic
        Debug.Log("Compete Phase Started");
        
        // Compete Phase End
        battlePhaseCoordinator.CompleteCompeteStart();
    }
    
    private void CompetePhaseMiddleProcess()
    {
        // Compete Phase Logic
        Debug.Log("Compete Phase Performing");
        
        // Compete Phase End
        battlePhaseCoordinator.CompleteCompetePerform();
    }
    
    private void CompetePhaseEndProcess()
    {
        // Compete Phase Logic
        Debug.Log("Compete Phase Ended");
        
        // Compete Phase End
        battlePhaseCoordinator.CompleteCompeteEnd();
    }
}
