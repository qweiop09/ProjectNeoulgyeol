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
        
        // CharacterBattleData actPlayerCharacterBattleData = actData.CastPlayerCharacter.GetCharacterBattleData();

        // 선택된 행동의 타겟팅 데이터를 저장 (배열의 편성 순서에 맞게)
        
        // _playerCharacterTargetingData
        //         [actPlayerCharacterBattleData.PlacementOrder].
        //     GetCharacterBattleData().TargetingData[actData.UseSlot] = actData;
        
        // 타겟팅 데이터 반영
        _playerCharacterTargetingData[characterHandler.GetCharacterBattleData().TurnOrder] = characterHandler;
        
        // Debug.Log($"Updated Targeting Data for {actPlayerCharacterBattleData.CharacterData.name} at Slot {characterHandler.UseSlot}");

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
    
    // / 여기까지 검수함
    
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
