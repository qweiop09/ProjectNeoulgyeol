using _01_Scripts.DTO;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    // 한 슬롯에서 캐스터가 취할 수 있는 (스킬,타겟) 조합 하나. Skill==null이면 Stay를 의미한다.
    public readonly struct EnemyDecisionCandidate
    {
        public readonly CharacterSkill Skill;
        public readonly CharacterHandler MainTarget;

        public bool IsStay => Skill == null;

        public EnemyDecisionCandidate(CharacterSkill skill, CharacterHandler mainTarget)
        {
            Skill = skill;
            MainTarget = mainTarget;
        }

        public static EnemyDecisionCandidate Stay() => new EnemyDecisionCandidate(null, null);
    }
}
