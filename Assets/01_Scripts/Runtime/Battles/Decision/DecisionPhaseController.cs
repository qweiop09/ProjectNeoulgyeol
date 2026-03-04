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
        battlePhaseCoordinator.OnDecisionPhaseStart += DecisionPhaseStartProcess;
        battlePhaseCoordinator.OnDecisionPhasePerform += DecisionPhaseMiddleProcess;
        battlePhaseCoordinator.OnDecisionPhaseEnd += DecisionPhaseEndProcess;
    }
    
    private void DecisionPhaseStartProcess()
    {
        // Decision Phase Logic
        Debug.Log("Decision Phase Started");
        
        // Decision Phase End
        battlePhaseCoordinator.CompleteDecisionStart();
    }
    
    private void DecisionPhaseMiddleProcess()
    {
        // Decision Phase Logic
        Debug.Log("Decision Phase Performing");
        
        // Decision Phase End
        battlePhaseCoordinator.CompleteDecisionPerform();
    }
    
    private void DecisionPhaseEndProcess()
    {
        // Decision Phase Logic
        Debug.Log("Decision Phase Ended");
        
        // Decision Phase End
        battlePhaseCoordinator.CompleteDecisionEnd();
    }
    
}
