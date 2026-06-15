// using _01_Scripts.Timeline.Battle.Receiver;
// using UnityEngine;
//
// public class CharacterAnimationController : MonoBehaviour
// {
//     [SerializeField] private Animator animator;
//     
//     [SerializeField] private AnimationMarkerReceiver markerReceiver;
//
//     public void Awake()
//     {
//         markerReceiver.OnAnimationMarkerReceived += HandleAnimationMarker;
//     }
//     
//     private void HandleAnimationMarker(string markerMessage)
//     {
//         Debug.Log($"CharacterAnimationController: BattleMarker 수신 - {markerMessage}");
//         
//         // markerMessage에 따라 애니메이션 트리거 설정
//         switch (markerMessage)
//         {
//             case "AttackStart":
//                 // animator.SetBool("IsAttacking", true);
//                 animator.SetTrigger("IsAttacking");
//                 break;
//             
//             case "IsRuning":
//                 animator.SetBool("IsRuning", true);
//                 break;
//             
//             case "IsNotRuning":
//                 animator.SetBool("IsRuning", false);
//                 break;
//             
//             case "UseSkill":
//                 animator.SetTrigger("UseSkill");
//                 break;
//             
//         }
//     }
// }
