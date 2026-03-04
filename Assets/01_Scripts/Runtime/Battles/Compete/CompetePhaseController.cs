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
        
        battlePhaseCoordinator.OnCompetePhaseStart += StartCompetePhase;
        battlePhaseCoordinator.OnCompetePhasePerform += StartCompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd += StartCompetePhaseEndProcess;
    }
    
    // Start Phase Actions
    private void StartCompetePhase()
    {
        CompleteCompetePhaseStartProcess();
    }
    
    private void CompleteCompetePhaseStartProcess()
    {
        Debug.Log("Compete Phase Started");
        battlePhaseCoordinator.CompleteCompeteStart();
    }
    
    
    // Middle Phase Actions
    private void StartCompetePhaseMiddleProcess()
    {
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
        // Compete Phase Logic
        Debug.Log("Compete Phase Ended");
        
        // Compete Phase End
        battlePhaseCoordinator.CompleteCompeteEnd();
    }
}
