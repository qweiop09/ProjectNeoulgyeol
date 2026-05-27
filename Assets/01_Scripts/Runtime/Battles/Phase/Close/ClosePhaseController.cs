using System.Threading.Tasks;
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
    
    private void StartClosePhase()
    {
        CompleteClosePhaseProcess();
    }
    
    private async void CompleteClosePhaseProcess()
    {
        // Close Phase Logic
        Debug.Log("Close Phase Ended");
        await Wait(1.2f); 
        
        // Close Phase End
        battlePhaseCoordinator.CompleteClosePhaseEnd();
    }
    
    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }
    
    
}
}
