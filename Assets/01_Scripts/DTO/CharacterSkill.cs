using UnityEngine;

namespace _01_Scripts.DTO
{
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/SkillData", fileName = "New Skill Data")]
public class CharacterSkill : ScriptableObject
{
    // 뭔가 나중에 쓸 것 같음
    // [SerializeField] private int skillStartPriority; // 스킬의 행동 우선순위 (낮을수록 먼저 행동)
    
    [SerializeField] public int skillStartDistance; // 스킬의 행동 시작 거리 타입 (근거리, 원거리 등)
    
    
}
}
