using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Interaction
{
    // 필드에 세워둔 적(보스 등)과 상호작용하면 고정 편성으로 전투를 시작한다.
    public class FieldEncounterInteractable : WorldInteractable
    {
        [SerializeField] private FixedEncounterData encounter;

        // encounter를 안 붙였으면 상호작용 프롬프트 자체를 안 띄운다(눌러도 아무 일도 안 나는 상태를 방지)
        public override bool CanInteract => encounter != null;

        public override void Interact(PlayerInteractionController interactor)
            => EncounterManager.Instance.StartFixedEncounter(encounter);
    }
}
