using _01_Scripts.DTO;
using _01_Scripts.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Battles
{
public class CharacterHandler : MonoBehaviour
{
    public enum CharacterType
    {
          Friendly
        , Enemy
    }

    [SerializeField] public TimelineDirector timelineDirector;
    [SerializeField] public PlayableDirector director;

    [SerializeField] public CharacterType characterType;
    
    // 애니메니터 (대기모션 등 타임라인 재생이 필요없는 애니메이션을 관리)
    // [SerializeField] public CharacterAnimationMonitor animationMonitor;
    [SerializeField] public Animator animator;
    
    // 임시
    [SerializeField] public QTEListner qteListner;

    // 아이템 사용 시 손에 든 아이템을 보여주는 상시 슬롯 (평소엔 비활성, ItemTimelineBinder가 재생 시점에 스프라이트를 채우고 타임라인의 ItemHoldTrack이 켜고 끔)
    [SerializeField] public SpriteRenderer heldItemRenderer;

    // 캐릭터 본체 스프라이트 — 버프 오라 파티클(CharacterBuffAuraController)의 Shape가 이 캐릭터의 실루엣을 따라가도록 런타임에 연결해준다.
    [SerializeField] public SpriteRenderer bodyRenderer;

    // 타임라인의 CasterAudioTrack/TargetAudioTrack이 바인딩할 스피커 (프리팹에 고정 배치)
    [SerializeField] public AudioSource sfxSource;

    [Space(10)]
    [Header("internal fields")]

    [SerializeField] public CharacterStatus characterStatus;

    // 전투 1회용 데이터 (이 캐릭터가 이번 전투에서만 갖는 값들, CharacterStatus에는 포함되지 않음)
    public int CurrentSpeed;
    public int PlacementOrder;
    public int TurnOrder;
    public int FormationIndex; // 전투 시작 시 한 번만 설정되는 원래 편성 순서 — 매 라운드 속도 재정렬과 무관한 안정적 타이브레이크 기준
    public ActData[] TargetingData;

    public void SetCharacterStatus(CharacterStatus _characterStatus)
    {
        characterStatus = _characterStatus;
    }
    public CharacterStatus GetCharacterStatus() => characterStatus;

    public void SetRandomSpeed()
    {
        CurrentSpeed = characterStatus.CharacterData.GetRandomSpeed();
    }

    public void DebugPrintStatusData()
    {
        Debug.Log(
            "체력 : " + characterStatus.CharacterData.maxHp + "\n" +
            "마나 : " + characterStatus.CharacterData.maxMp + "\n" +
            "공격 : " + characterStatus.CharacterData.attack + "\n" +
            "방어 : " + characterStatus.CharacterData.defense + "\n" +
            "현재 속도 : " + CurrentSpeed);
    }

}
}
