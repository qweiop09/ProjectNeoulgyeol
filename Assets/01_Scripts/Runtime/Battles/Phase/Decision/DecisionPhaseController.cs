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
    
    private void Awake()
    {
        _battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        _battlePhaseCoordinator.OnDecisionPhaseStart += StartDecisionPhase;
        _battlePhaseCoordinator.OnDecisionPhasePerform += StartDecisionPhaseMiddleProcess;
        _battlePhaseCoordinator.OnDecisionPhaseEnd += StartDecisionPhaseEndProcess;
        
        actionSelectionPhaseManager.CompleteActSelected += SetSelectedActData;
    }
    
    private void SetSelectedActData(ActData actData)
    {
        Debug.Log("Setting Selected Act Data: " + actData);
        
        // 타겟팅 데이터 반영
        Debug.Log(actData.CastPlayerCharacter.GetCharacterBattleData().TurnOrder);
        Debug.Log(_turnOrderCharacters.Length);
        
        Debug.Log(actData.UseSlot);
        Debug.Log(actData.CastPlayerCharacter.GetCharacterBattleData().TargetingData.Length);
        
        _turnOrderCharacters
            [actData.CastPlayerCharacter.GetCharacterBattleData().TurnOrder]
            .GetCharacterBattleData().TargetingData[actData.UseSlot] = actData;
    }
    
    
    // Start Phase Actions
    private void StartDecisionPhase(CharacterHandler[] turnOrderCharacters)    
    {
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
        
        _battlePhaseCoordinator.CompleteDecisionEnd(_turnOrderCharacters);
        
        
    }
    
}
}
