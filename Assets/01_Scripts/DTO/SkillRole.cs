namespace _01_Scripts.DTO
{
    // 적 AI가 스킬을 역할별로 스코어링/필터링하는 데 쓰는 태그. 플레이어 UI엔 영향 없음.
    public enum SkillRole
    {
        Attack,
        Heal,
        Buff,
        Debuff,
        Utility
    }
}
