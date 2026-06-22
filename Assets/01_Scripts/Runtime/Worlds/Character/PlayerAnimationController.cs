using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Character
{
    [RequireComponent(typeof(PlayerCharacterMove))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        // Animator 파라미터 해시 (문자열 조회 비용 절감)
        private static readonly int IsMoving  = Animator.StringToHash("IsMoving");
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int MoveX     = Animator.StringToHash("MoveX");
        private static readonly int MoveY     = Animator.StringToHash("MoveY");

        private PlayerCharacterMove _move;

        private void Awake()
        {
            _move = GetComponent<PlayerCharacterMove>();
        }

        private void Update()
        {
            var dir = _move.MoveDirection;
            bool isMoving = dir.magnitude > 0.1f;

            _animator.SetBool(IsMoving,  isMoving);
            _animator.SetBool(IsRunning, isMoving && _move.IsRunning);
            _animator.SetFloat(MoveX,    dir.x);
            _animator.SetFloat(MoveY,    dir.y);
        }
    }
}
