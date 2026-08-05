namespace _01_Scripts.DTO.Item
{
    // 캐릭터에게 걸린 지속 효과 한 건. 수치/이름/아이콘은 Source(에셋) 쪽에 있고,
    // 여기는 "그 소스가 걸려있다"는 사실과 확정된 크기, 남은 라운드만 들고 있는다.
    public class ActiveBuff
    {
        public BuffEffectBase Source;   // 같은 에셋 참조면 같은 버프로 취급(재적용 시 갱신)
        public int ResolvedAmount;      // Apply 시점에 확정된 값 — 지속 중에는 재계산하지 않음
        public int RemainingRounds;
    }
}
