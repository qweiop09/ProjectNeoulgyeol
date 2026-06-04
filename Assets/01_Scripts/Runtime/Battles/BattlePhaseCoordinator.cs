using System;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
public class BattlePhaseCoordinator : MonoBehaviour
{
    private enum PhaseState
    {
          Open
        , Decision
        , Compete
        , Close
    }

    [SerializeField] private PhaseState currentPhase = PhaseState.Open;
   
    // 송신
    // 시작 메세지
    // Battle Start Actions
    public event Action<CharacterHandler[], CharacterHandler[]> OnBattleStart;
   
    // Open Phase Actions
    public event Action OnOpenPhaseStart;  
   
    // Decision Phase Actions
    public event Action<CharacterHandler[], CharacterHandler[], CharacterHandler[]> OnDecisionPhaseStart;
    public event Action OnDecisionPhasePerform;             
    public event Action OnDecisionPhaseEnd;                
   
    // Compete Phase Actions
    public event Action<CharacterHandler[], CharacterHandler[]> OnCompetePhaseStart;
    public event Action OnCompetePhasePerform;
    public event Action OnCompetePhaseEnd;
   
    // Close Phase Actions
    public event Action<CharacterHandler[]> OnClosePhaseEnd;
   
    // 수신
    // Start Battle
    public void BattleStart(CharacterHandler[] playerBattleData, CharacterHandler[] enemyBattleData)
    {
        currentPhase = PhaseState.Open; 

        OnBattleStart?.Invoke(playerBattleData, enemyBattleData);
    }
   
    // Open Phase Actions
    public void CompleteOpenPhaseStart(CharacterHandler[] playerBattleData, CharacterHandler[] enemyBattleData, CharacterHandler[] turnOrderCharacters)
    {
        if( currentPhase != PhaseState.Open) return;
        currentPhase = PhaseState.Decision;
        
        OnDecisionPhaseStart?.Invoke(playerBattleData, enemyBattleData, turnOrderCharacters);
    }
    
    // Decision Phase Actions
    public void CompleteDecisionStart()
    {
        if( currentPhase != PhaseState.Decision) return;
        OnDecisionPhasePerform?.Invoke();
    }
    
    public void CompleteDecisionPerform()
    {
        if( currentPhase != PhaseState.Decision) return;
        OnDecisionPhaseEnd?.Invoke();
    }
    
    public void CompleteDecisionEnd(CharacterHandler[] playerBattleData, CharacterHandler[] enemyBattleData)
    {
        if( currentPhase != PhaseState.Decision) return;
        currentPhase = PhaseState.Compete;
        
        OnCompetePhaseStart?.Invoke(playerBattleData, enemyBattleData);
    }
    
    // Compete Phase Actions
    public void CompleteCompeteStart()
    {
        if( currentPhase != PhaseState.Compete) return;
        OnCompetePhasePerform?.Invoke();
    }
    
    public void CompleteCompetePerform()
    {
        if( currentPhase != PhaseState.Compete) return;
        OnCompetePhaseEnd?.Invoke();
    }
    
    public void CompleteCompeteEnd(CharacterHandler[] allCharacterHandlers)
    {
        if( currentPhase != PhaseState.Compete) return;
        currentPhase = PhaseState.Close;
        
        OnClosePhaseEnd?.Invoke(allCharacterHandlers);
    }
    
    // Close Phase Actions
    public void CompleteClosePhaseEnd()
    {
        if( currentPhase != PhaseState.Close) return;
        currentPhase = PhaseState.Open;
        
        OnOpenPhaseStart?.Invoke();
    }

}
}
