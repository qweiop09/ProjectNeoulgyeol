using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Decision
{
public class DecisionPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator _battlePhaseCoordinator;
    
    [SerializeField] private ActionSelectionPhaseManager actionSelectionPhaseManager;
    
    private CharacterHandler[] _turnOrderCharacters;
    private CharacterHandler[] _playerCharacterTargetingData;
    private CharacterHandler[] _enemyCharacterTargetingData;
    
    
    private void Awake()
    {
        _battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        _battlePhaseCoordinator.OnDecisionPhaseStart += StartDecisionPhase;
        _battlePhaseCoordinator.OnDecisionPhasePerform += StartDecisionPhaseMiddleProcess;
        _battlePhaseCoordinator.OnDecisionPhaseEnd += StartDecisionPhaseEndProcess;
        
        actionSelectionPhaseManager.OnActSelected += SetSelectedActData;
    }
    
    private void SetSelectedActData(CharacterHandler characterHandler)
    {
        Debug.Log("Setting Selected Act Data: " + characterHandler);
        
        // 타겟팅 데이터 반영
        _playerCharacterTargetingData[characterHandler.GetCharacterBattleData().TurnOrder] = characterHandler;
        
    }
    
    
    // Start Phase Actions
    private void StartDecisionPhase(CharacterHandler[] playerBattleData, CharacterHandler[] enemyBattleData, CharacterHandler[] turnOrderCharacters)    {
        
        _playerCharacterTargetingData = playerBattleData;
        _enemyCharacterTargetingData = enemyBattleData;
        
        _turnOrderCharacters = turnOrderCharacters;
        
        CompleteDecisionPhaseStartProcess();
    }
    
    private void CompleteDecisionPhaseStartProcess()
    {
        Debug.Log("Decision Phase Started");
        _battlePhaseCoordinator.CompleteDecisionStart();
    }
    
    // Middle Phase Actions
    private void StartDecisionPhaseMiddleProcess()
    {
        actionSelectionPhaseManager.StartActionSelectionPhase();
    }

    public void PressedCompetePhaseStartButton()
    {
        CompleteDecisionPhaseMiddleProcess();
    }
    
    private void CompleteDecisionPhaseMiddleProcess()
    {
        Debug.Log("Decision Phase Performing");
        actionSelectionPhaseManager.EndActionSelectionPhase();
        
        _battlePhaseCoordinator.CompleteDecisionPerform();
    }
    
    // End Phase Actions
    private void StartDecisionPhaseEndProcess()
    {
        CompleteDecisionPhaseEndProcess();
    }
    
    private void CompleteDecisionPhaseEndProcess()
    {
        Debug.Log("Decision Phase Ended");
        
        _battlePhaseCoordinator.CompleteDecisionEnd(_playerCharacterTargetingData, _enemyCharacterTargetingData);
        
        
    }
    
}
}
