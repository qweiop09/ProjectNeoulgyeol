using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 버프 아이콘 한 개. DamageText.cs와 같은 방식(Animator 없이 코루틴으로 스케일/알파를 직접 보간)으로 연출한다.
public class CharacterBuffIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI remainingRoundsText; // 선택 — 비워두면 라운드 수 표시를 생략

    [Header("연출")]
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private float pulseScale = 1.25f;
    [SerializeField] private float pulseDuration = 0.15f;

    private Coroutine animRoutine;

    public void Initialize(Sprite icon, int remainingRounds)
    {
        // 프리팹 루트가 비활성 상태로 저장되어 있어도(에디터 설정 실수 등) 여기서 스스로 깨운다 —
        // 안 그러면 아래 StartCoroutine이 "GameObject가 비활성"이라며 조용히 실패해서 팝인 애니메이션도,
        // 화면 표시도 안 된다.
        gameObject.SetActive(true);

        iconImage.sprite = icon;
        SetRemainingRounds(remainingRounds);

        transform.localScale = Vector3.zero;
        RestartAnimation(PopIn());
    }

    public void SetRemainingRounds(int remainingRounds)
    {
        if (remainingRoundsText != null)
            remainingRoundsText.text = remainingRounds.ToString();
    }

    // 같은 버프가 재부여(갱신)됐을 때 — 라운드 수는 호출부에서 SetRemainingRounds로 먼저 갱신해둔다
    public void PlayRefreshPulse()
    {
        RestartAnimation(Pulse());
    }

    // 만료 — 축소+페이드 후 자기 자신을 파괴한다
    public void PlayExpireAndDestroy()
    {
        RestartAnimation(PopOutAndDestroy());
    }

    private void RestartAnimation(IEnumerator routine)
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(routine);
    }

    private IEnumerator PopIn()
    {
        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            // 살짝 오버슈트했다가 정착 — 튀어나오는 느낌
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, eased * 1.1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    private IEnumerator Pulse()
    {
        Vector3 baseScale = Vector3.one;
        float halfDuration = pulseDuration * 0.5f;

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            transform.localScale = Vector3.Lerp(baseScale, baseScale * pulseScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            transform.localScale = Vector3.Lerp(baseScale * pulseScale, baseScale, t);
            yield return null;
        }

        transform.localScale = baseScale;
    }

    private IEnumerator PopOutAndDestroy()
    {
        Vector3 startScale = transform.localScale;
        Color startColor = iconImage.color;

        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            iconImage.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
