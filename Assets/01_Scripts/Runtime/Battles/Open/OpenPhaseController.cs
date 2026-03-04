using UnityEngine;

public class OpenPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnOpenPhaseStart += StartOpenPhase;
    }
    
    private void StartOpenPhase()
    {
        CompleteOpenPhaseProcess();
    }
    
    private void CompleteOpenPhaseProcess()
    {
        // Open Phase Logic
        Debug.Log("Open Phase Started");
        
        // Open Phase End
        battlePhaseCoordinator.CompleteOpenPhaseStart();
    }
    
    
}
