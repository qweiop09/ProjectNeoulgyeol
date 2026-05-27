using _01_Scripts.Timeline.Battle.Receiver;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    [SerializeField] private BattleMarkerReceiver markerReceiver;

    public void Awake()
    {
        markerReceiver.OnBattleMarkerReceived += HandleBattleMarker;
    }
    
    private void HandleBattleMarker(string markerMessage)
    {
        Debug.Log($"CharacterAnimationController: BattleMarker 수신 - {markerMessage}");
        
        // markerMessage에 따라 애니메이션 트리거 설정
        switch (markerMessage)
        {
            case "AttackStart":
                // animator.SetBool("IsAttacking", true);
                animator.SetTrigger("IsAttacking");
                break;
        }
    }
}
