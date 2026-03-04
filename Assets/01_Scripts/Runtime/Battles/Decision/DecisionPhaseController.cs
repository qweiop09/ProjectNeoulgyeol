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
        battlePhaseCoordinator.CompleteDecisionEnd();
    }
    
}
