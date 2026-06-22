using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Worlds.Interaction
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private float _interactRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private Key _interactKey = Key.E;
        [SerializeField] private InteractionHintUI _hintUI;

        private IInteractable _nearest;
        private readonly Collider2D[] _buffer = new Collider2D[8];

        private void Update()
        {
            UpdateNearest();
            HandleInput();
        }

        private void UpdateNearest()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, _interactRadius, _buffer, _interactableLayer);

            IInteractable nearest = null;
            float minDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].TryGetComponent<IInteractable>(out var interactable)) continue;

                float dist = Vector2.Distance(transform.position, _buffer[i].transform.position);
                if (dist >= minDist) continue;

                minDist = dist;
                nearest = interactable;
            }

            if (_nearest == nearest) return;

            _nearest = nearest;
            RefreshHint();
        }

        private void HandleInput()
        {
            if (_nearest == null || !_nearest.CanInteract) return;
            if (Keyboard.current != null && Keyboard.current[_interactKey].wasPressedThisFrame)
                _nearest.Interact(this);
        }

        private void RefreshHint()
        {
            if (_hintUI == null) return;

            if (_nearest != null && _nearest.CanInteract)
                _hintUI.Show(_nearest.InteractionHint);
            else
                _hintUI.Hide();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactRadius);
        }
    }
}
