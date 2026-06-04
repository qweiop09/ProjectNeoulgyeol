using System.Threading.Tasks;
using _01_Scripts.Runtime.Battles.CameraControlle;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Close
{
public class ClosePhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnClosePhaseEnd += StartClosePhase;
    }
    
    private void StartClosePhase(CharacterHandler[] allCharacterHandlers )
    {
        CompleteClosePhaseProcess(allCharacterHandlers);
    }
    
    private async void CompleteClosePhaseProcess(CharacterHandler[] allCharacterHandlers)
    {
        // Close Phase Logic
        
        for(int i = 0; i < allCharacterHandlers.Length; i++)
        {
            Debug.Log("Character " + i + ": " + allCharacterHandlers[i].name);
            allCharacterHandlers[i].GetCharacterBattleData().TargetingData
                = new ActData[allCharacterHandlers[i].GetCharacterBattleData().TargetingData.Length];
        }
        
        
        Debug.Log("Close Phase Ended");
        await Wait(0.4f);

        await CameraHandler.Instance.PositionResetToLerp();
        
        // Close Phase End
        battlePhaseCoordinator.CompleteClosePhaseEnd();
    }
    
    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }
    
    
}
}
