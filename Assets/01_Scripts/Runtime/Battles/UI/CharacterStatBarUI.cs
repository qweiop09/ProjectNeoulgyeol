using System.Collections;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatBarUI : MonoBehaviour
{
    [SerializeField] private Canvas barCanvas; // overrideSorting=true, sortingOrder 개별 제어용
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private CharacterBuffIconRowUI buffIconRow; // hpSlider 위에 배치되는 버프 아이콘 줄

    [Header("잔상(딜레이) 바 — 데미지 입었을 때 한 박자 늦게 따라 내려옴")]
    [SerializeField] private Slider hpTrailSlider;
    [SerializeField] private Slider staminaTrailSlider;
    [SerializeField] private float trailDelay = 0.4f;         // 감소 시작 전 대기 시간
    [SerializeField] private float trailCatchUpDuration = 0.3f; // 따라잡는 데 걸리는 시간

    [SerializeField] private float dockMoveDuration = 0.25f;

    private CharacterHandler target;
    private Transform worldParent;      // 평상시(추적 상태) 부모 — BattleUI 밑
    private Vector3 originalLocalScale; // Undock 시 복귀할 스케일
    private Vector3 worldOffset;        // CharacterData.statBarOffset에서 캐싱 (캐릭터별로 다름)
    private bool isDocked;
    private Coroutine moveRoutine;
    private Coroutine hpTrailRoutine;
    private Coroutine staminaTrailRoutine;
    private WaitForSeconds trailDelayWait; // 매 타격마다 new WaitForSeconds 할당하지 않도록 캐싱

    public void Initialize(CharacterHandler handler, Transform defaultParent)
    {
        target = handler;
        worldParent = defaultParent;
        worldOffset = target.GetCharacterStatus().CharacterData.statBarOffset;
        transform.SetParent(worldParent, false);
        originalLocalScale = transform.localScale;

        gameObject.SetActive(true);
        SnapValues(target.GetCharacterStatus()); // 최초 스폰 시엔 잔상 연출 없이 바로 맞춤
        buffIconRow?.Initialize(target);
    }

    private void OnEnable()
    {
        CharacterStatusCalculator.Instance.onStatusChanged += HandleStatusChanged;
        CharacterStatusCalculator.Instance.isCharacterDead += HandleCharacterDead;
    }

    private void OnDisable()
    {
        if (CharacterStatusCalculator.Instance == null) return; // 씬 종료 중 순서 이슈 방지
        CharacterStatusCalculator.Instance.onStatusChanged -= HandleStatusChanged;
        CharacterStatusCalculator.Instance.isCharacterDead -= HandleCharacterDead;
    }

    private void LateUpdate()
    {
        if (target == null || isDocked) return;
        transform.position = target.transform.position + worldOffset; // 회전은 상속 안 받음 (독립 오브젝트)
    }

    private void HandleStatusChanged(CharacterStatus status)
    {
        if (target == null || status != target.GetCharacterStatus()) return;
        UpdateValues(status);
    }

    private void HandleCharacterDead(CharacterStatus status)
    {
        if (target == null || status != target.GetCharacterStatus()) return;
        gameObject.SetActive(false);
    }

    private void SnapValues(CharacterStatus status)
    {
        float hpRatio = status.currentHp / (float)status.GetMaxHp();
        float staminaRatio = status.currentStamina / (float)status.GetMaxStamina();

        hpSlider.value = hpRatio;
        hpTrailSlider.value = hpRatio;
        staminaSlider.value = staminaRatio;
        staminaTrailSlider.value = staminaRatio;
    }

    private void UpdateValues(CharacterStatus status)
    {
        float hpRatio = status.currentHp / (float)status.GetMaxHp();
        float staminaRatio = status.currentStamina / (float)status.GetMaxStamina();

        hpTrailRoutine = ApplyValue(hpSlider, hpTrailSlider, hpRatio, hpTrailRoutine);
        staminaTrailRoutine = ApplyValue(staminaSlider, staminaTrailSlider, staminaRatio, staminaTrailRoutine);
    }

    // 앞쪽 바는 즉시 실제 수치로 스냅. 값이 줄어드는(데미지) 경우에만 뒤쪽 잔상 바가 한 박자 늦게 따라 내려온다.
    // 회복 등 값이 늘어나는 경우는 잔상 없이 같이 스냅.
    private Coroutine ApplyValue(Slider front, Slider trail, float newValue, Coroutine currentTrailRoutine)
    {
        bool isDamage = newValue < front.value;
        front.value = newValue;

        if (currentTrailRoutine != null) StopCoroutine(currentTrailRoutine);

        if (!isDamage)
        {
            trail.value = newValue;
            return null;
        }

        return StartCoroutine(TrailCatchUpRoutine(trail, newValue));
    }

    private IEnumerator TrailCatchUpRoutine(Slider trail, float targetValue)
    {
        trailDelayWait ??= new WaitForSeconds(trailDelay);
        yield return trailDelayWait;

        float startValue = trail.value;
        float t = 0f;
        while (t < trailCatchUpDuration)
        {
            t += Time.deltaTime;
            trail.value = Mathf.Lerp(startValue, targetValue, Mathf.Clamp01(t / trailCatchUpDuration));
            yield return null;
        }

        trail.value = targetValue;
    }

    public void DockTo(Transform slot)
    {
        RestartMove(DockRoutine(slot));
    }

    public void Undock()
    {
        RestartMove(UndockRoutine());
    }

    // 코루틴이 도는 동안(Dock/Undock 둘 다)은 LateUpdate가 위치를 건드리면 안 되므로 항상 먼저 true로 잠근다.
    // Dock은 끝나고도 계속 true 유지(메뉴에 붙어있는 상태), Undock만 끝날 때 자기 손으로 false로 풀어준다.
    private void RestartMove(IEnumerator routine)
    {
        isDocked = true;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(routine);
    }

    private IEnumerator DockRoutine(Transform slot)
    {
        transform.SetParent(slot, true); // worldPositionStays: 현재 화면 위치 유지한 채 부모만 교체
        Vector3 startLocalPos = transform.localPosition;
        Vector3 startLocalScale = transform.localScale;

        float t = 0f;
        while (t < dockMoveDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dockMoveDuration);
            transform.localPosition = Vector3.Lerp(startLocalPos, Vector3.zero, p);
            transform.localScale = Vector3.Lerp(startLocalScale, Vector3.one, p);
            yield return null;
        }

        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    private IEnumerator UndockRoutine()
    {
        transform.SetParent(worldParent, true); // worldPositionStays: 현재 화면 위치 유지한 채 부모만 교체
        Vector3 startWorldPos = transform.position;
        Vector3 startLocalScale = transform.localScale;

        float t = 0f;
        while (t < dockMoveDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dockMoveDuration);
            // 캐릭터가 그 사이 움직였을 수도 있으니 목표 위치를 매 프레임 다시 계산
            Vector3 endWorldPos = target.transform.position + worldOffset;
            transform.position = Vector3.Lerp(startWorldPos, endWorldPos, p);
            transform.localScale = Vector3.Lerp(startLocalScale, originalLocalScale, p);
            yield return null;
        }

        isDocked = false; // 여기서부터 LateUpdate가 추적을 이어받음
    }
}
