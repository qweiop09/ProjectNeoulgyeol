using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class WorldInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string _hintText = "조사";

        public virtual string InteractionHint => _hintText;
        public virtual bool CanInteract => true;

        public abstract void Interact(PlayerInteractionController interactor);
    }
}
