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
    
    private void CompleteClosePhaseProcess()
    {
        // Close Phase Logic
        Debug.Log("Close Phase Ended");
        
        // Close Phase End
        battlePhaseCoordinator.CompleteClosePhaseEnd();
    }
    
    
}
}
