namespace _01_Scripts.Runtime.Worlds.Interaction
{
    public interface IInteractable
    {
        string InteractionHint { get; }
        bool CanInteract { get; }
        void Interact(PlayerInteractionController interactor);
    }
}
