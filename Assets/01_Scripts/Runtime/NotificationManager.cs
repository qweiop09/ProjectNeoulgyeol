using System.Collections;
using TMPro;
using UnityEngine;
using _01_Scripts.Interfacese;

// 짧은 안내 메시지를 화면에 띄우는 공용 토스트. "도망 불가", "인벤토리 꽉 참", "구매 실패" 등
// 어디서든 같은 모양으로 떠야 하는 1회성 안내 문구를 전부 여기로 모은다.
// Singleton(DontDestroyOnLoad)이라 Battle/World 씬을 오가도 같은 인스턴스가 유지된다 — 처음 로드되는 씬에
// 프리팹 하나만 배치해두면 됨(DamageTextSpawner 등과 동일한 배치 방식).
public class NotificationManager : Singleton<NotificationManager>
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayDuration = 2f;

    private Coroutine hideRoutine;

    // message가 비어있으면 아무것도 하지 않는다(호출부에서 조건 없이 편하게 부를 수 있도록)
    public void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (messageText == null || panel == null)
        {
            Debug.Log($"[NotificationManager] {message}");
            return;
        }

        messageText.text = message;
        panel.SetActive(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void Hide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (panel != null)
            panel.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (panel != null)
            panel.SetActive(false);
        hideRoutine = null;
    }
}
