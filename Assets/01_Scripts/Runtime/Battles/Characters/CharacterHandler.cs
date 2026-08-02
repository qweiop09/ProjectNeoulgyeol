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

    [Space(10)]
    [Header("internal fields")]

    [SerializeField] public CharacterStatus characterStatus;

    // 전투 1회용 데이터 (이 캐릭터가 이번 전투에서만 갖는 값들, CharacterStatus에는 포함되지 않음)
    public int CurrentSpeed;
    public int PlacementOrder;
    public int TurnOrder;
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
