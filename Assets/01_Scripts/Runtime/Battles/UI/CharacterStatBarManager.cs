using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

public class CharacterStatBarManager : Singleton<CharacterStatBarManager>
{
    [SerializeField] private CharacterStatBarUI barPrefab;
    [SerializeField] private Transform worldBarParent; // BattleUI 캔버스 하위
    [SerializeField] private Transform menuDockSlot;   // ComandMenu 하위, 도킹 자리

    [Tooltip("모든 캐릭터의 체력바에 공통으로 더해지는 오프셋. 캐릭터별 CharacterData.statBarOffset과 합산됨 — " +
             "전체 바 위치를 한 번에 옮기고 싶을 때 캐릭터 에셋을 하나하나 안 고치고 여기서 일괄 조정.")]
    [SerializeField] private Vector3 globalStatBarOffset;
    public Vector3 GlobalStatBarOffset => globalStatBarOffset;

    [Header("깊이 정렬 (Y좌표 기준)")]
    [Tooltip("Y가 제일 낮은(화면 제일 아래/카메라에 가장 가까운) 캐릭터의 sortingOrder. 뒤로 갈수록 rankOrderStep씩 빠짐")]
    [SerializeField] private int baseSortingOrder = 1000;
    [Tooltip("정렬 순위 1당 sortingOrder 간격 — barOverSpriteOffset보다 커야 인접 순위끼리 캐릭터/체력바 순서가 안 겹침")]
    [SerializeField] private int rankOrderStep = 10;
    [Tooltip("체력바가 자기 캐릭터 스프라이트보다 정확히 이만큼 위(sortingOrder+)에 오도록 — 그 외 순서는 캐릭터와 완전히 동일하게 묶여서 움직임")]
    [SerializeField] private int barOverSpriteOffset = 1;

    private readonly Dictionary<CharacterHandler, CharacterStatBarUI> bars = new();

    // 매 프레임 현재 Y좌표로 다시 정렬한다 — 캐릭터가 움직이는 동안(이동/공격 등)에도 자연스럽게 앞뒤가 갱신되도록.
    // 산 캐릭터 전원이 죽은 캐릭터보다 항상 앞, 그 안에서는 Y가 낮을수록(화면 아래쪽) 앞.
    // Y가 완전히 같으면(같은 슬롯 열 등) 예전 속도 기반 규칙으로 타이브레이크: 속도 내림차순 → 동속도면 아군 우선 → 그래도 같으면 편성순서.
    private void LateUpdate()
    {
        if (bars.Count == 0) return;

        List<CharacterHandler> sorted = bars.Keys
            .Where(c => c != null)
            .OrderBy(c => c.GetCharacterStatus().currentState == CharacterState.Dead ? 1 : 0)
            .ThenBy(c => c.transform.position.y)
            .ThenByDescending(c => c.CurrentSpeed)
            .ThenBy(c => c.characterType == CharacterHandler.CharacterType.Friendly ? 0 : 1)
            .ThenBy(c => c.FormationIndex)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
            SetOrder(sorted[i], baseSortingOrder - i * rankOrderStep);
    }

    // 캐릭터 스프라이트와 체력바를 한 쌍으로 묶어서 같은 order를 준다(체력바만 자기 스프라이트보다 barOverSpriteOffset만큼 위) —
    // 그래야 "체력바 전용 순서 구간"과 "스프라이트 전용 순서 구간"을 따로 관리하다 경계를 잘못 잡는 버그가 아예 생기지 않는다.
    private void SetOrder(CharacterHandler c, int order)
    {
        SpriteRenderer sprite = c.GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.sortingOrder = order;

        if (bars.TryGetValue(c, out var bar))
            bar.SetSortingOrder(order + barOverSpriteOffset);
    }

    public void SpawnBars(CharacterHandler[] playerCharacters, CharacterHandler[] enemyCharacters)
    {
        foreach (var handler in playerCharacters) Spawn(handler);
        foreach (var handler in enemyCharacters) Spawn(handler);
    }

    private void Spawn(CharacterHandler handler)
    {
        // 프리팹/씬 배치가 아직 안 되어 있으면(에디터 작업 전) 전투 자체가 막히지 않도록 건너뛴다.
        if (barPrefab == null || worldBarParent == null)
        {
            Debug.LogWarning("[CharacterStatBarManager] barPrefab/worldBarParent가 설정되지 않아 체력바를 생성하지 않습니다.");
            return;
        }

        CharacterStatBarUI bar = Instantiate(barPrefab);
        bar.Initialize(handler, worldBarParent);
        bars[handler] = bar;
    }

    public void ClearBars()
    {
        foreach (var bar in bars.Values)
            if (bar != null) Destroy(bar.gameObject);

        bars.Clear();
    }

    public void DockBarFor(CharacterHandler handler)
    {
        if (handler != null && bars.TryGetValue(handler, out var bar))
            bar.DockTo(menuDockSlot);
    }

    public void UndockBarFor(CharacterHandler handler)
    {
        if (handler != null && bars.TryGetValue(handler, out var bar))
            bar.Undock();
    }
}
