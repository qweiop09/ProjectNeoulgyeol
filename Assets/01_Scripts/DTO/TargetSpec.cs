namespace _01_Scripts.DTO
{
    // 누구를 고를 수 있는지
    public enum TargetScope
    {
        Self,
        Any,
        Ally,
        Enemy,
    }

    // 몇 명에게 적용되는지 (Self는 이 값을 무시하고 항상 캐스터 자신 1명)
    public enum TargetCount
    {
        Single,
        Fixed,
        All,
        Random,
    }

    // 아이템/스킬(공격 포함) 등 "누군가를 타겟팅하는 행동"이 공통으로 갖는 타겟 지정 정보.
    // Item과 CharacterSkill이 각자 구현하고, ActionSelectionPhaseManager/AttackArrowController는
    // 구체 타입(Item인지 CharacterSkill인지)을 몰라도 이 인터페이스만으로 타겟팅을 처리한다.
    public interface ITargetSpec
    {
        TargetScope TargetScope { get; }
        TargetCount TargetCount { get; }
        int TargetCountValue { get; }
    }
}
