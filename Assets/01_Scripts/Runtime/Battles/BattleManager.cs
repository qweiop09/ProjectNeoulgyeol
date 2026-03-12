using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    // class Variables
    [SerializeField] private OpenPhaseController openPhaseController;
    [SerializeField] private DecisionPhaseController targettingPhaseController;
    [SerializeField] private CompetePhaseController competePhaseController;
    [SerializeField] private ClosePhaseController closePhaseController;
    
    [SerializeField] private BattlePhaseCoordinator battlePhaseCoordinator;
    
    // data Variables
    [SerializeField] private CharacterStatus[] playerCharacters;
    [SerializeField] private CharacterStatus[] enemyCharacters;
    
    
    // private Methods
    public void BattleStart()
    { 
        battlePhaseCoordinator.BattleStart();
    }
    
    // public Methods
     public BattlePhaseCoordinator GetBattlePhaseCoordinator()
    {
        return battlePhaseCoordinator;
    }

    public CharacterStatus[] GetPlayerCharacters()
    {
        return playerCharacters;
    }

    public CharacterStatus[] GetEnemyCharacters()
    {
        return enemyCharacters;
    }
    
}
