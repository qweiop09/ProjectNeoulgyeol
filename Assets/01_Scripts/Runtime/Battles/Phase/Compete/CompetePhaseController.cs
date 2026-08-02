using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class CompetePhaseController : MonoBehaviour
{
    // class Variables
    [SerializeField] private BattleManager battleManager; 
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    [SerializeField] private CompeteContestController competeContestController;
    
    // data Variables
    private CharacterHandler[] allCharacterHandlers;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnCompetePhaseStart   += StartCompetePhaseStartProcess;
        battlePhaseCoordinator.OnCompetePhasePerform += StartCompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd     += StartCompetePhaseEndProcess;
    }

    private void OnDestroy()
    {
        battlePhaseCoordinator.OnCompetePhaseStart   -= StartCompetePhaseStartProcess;
        battlePhaseCoordinator.OnCompetePhasePerform -= StartCompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd     -= StartCompetePhaseEndProcess;
    }
    
    // Start Phase Actions
    private void StartCompetePhaseStartProcess(CharacterHandler[] _allCharacterHandlers)
    {
        allCharacterHandlers = null;
        allCharacterHandlers = _allCharacterHandlers;

        CompleteCompetePhaseStartProcess();
    }
    
    private void CompleteCompetePhaseStartProcess()
    {
        Debug.Log("Compete Phase Started");
        battlePhaseCoordinator.CompleteCompeteStart();
    }
    
    // Middle Phase Actions
    private async void StartCompetePhaseMiddleProcess()
    {
        // 모든 캐릭터의 행동을 실행
        for (int i = 0; i < allCharacterHandlers.Count(); i++)
        {
            CharacterHandler currentCharacter = allCharacterHandlers[i];

            // Compete Cycle Phase
            // 한 캐릭터의 모든 행동을 실행
            await competeContestController.StartCompeteCycle(currentCharacter.TargetingData);
            Debug.Log("Compete Cycle Completed for Character: " + currentCharacter.name);
        }

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
        Debug.Log("Compete Phase Ended");

        var characters = allCharacterHandlers;
        allCharacterHandlers = null;

        battlePhaseCoordinator.CompleteCompeteEnd(characters);
    }
}
}
