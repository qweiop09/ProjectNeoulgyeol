using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClosePhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnClosePhaseEnd += ClosePhaseProcess;
    }
    
    private void ClosePhaseProcess()
    {
        // Close Phase Logic
        Debug.Log("Close Phase Ended");
        
        // Close Phase End
        // battlePhaseCoordinator.CompleteClosePhaseEnd();
    }
    
    
}
