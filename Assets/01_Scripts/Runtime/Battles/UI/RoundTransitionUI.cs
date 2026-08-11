using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.UI
{
// 라운드 시작 배너("ROUND N"). Battle 씬 전용이라 싱글톤 아님 — OpenPhaseController가 직접 참조.
// NotificationManager와 같은 패널+텍스트+자동 숨김 구조지만, 다른 연출(캐릭터 타임라인 등)과
// Task.WhenAll로 동시에 기다릴 수 있도록 ShowAsync가 Task를 반환한다.
public class RoundTransitionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private float displayDuration = 1.5f;

    private Coroutine hideRoutine;
    private TaskCompletionSource<bool> pendingTcs;

    public Task ShowAsync(int roundNumber)
    {
        var tcs = new TaskCompletionSource<bool>();

        if (roundText == null || panel == null)
        {
            tcs.SetResult(true); // 연출 미구성이어도 흐름은 막지 않는다 (NotificationManager와 동일한 관용구)
            return tcs.Task;
        }

        // 루트 오브젝트가 씬에 비활성화 상태로 남아있어도(에디터 설정 실수 등) 여기서 스스로 깨운다 —
        // 안 그러면 StartCoroutine이 조용히 실패해서 배너가 안 뜬다(LootUI/CharacterBuffIconUI와 동일 문제).
        gameObject.SetActive(true);

        // 직전 배너가 아직 안 사라졌는데 다시 호출되면(겹쳐서 발동 등) 그 코루틴부터 정리하고 새로 시작한다.
        // 이때 직전 호출이 반환한 Task를 누군가 await하고 있을 수 있으니, 취소만 하고 방치하면 그 Task가
        // 영원히 안 끝나서 Task.WhenAll이 멈춘다 — 여기서 먼저 완료 처리해준다.
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            pendingTcs?.TrySetResult(true);
        }

        roundText.text = $"ROUND {roundNumber}";
        panel.SetActive(true);
        roundText.enabled = true; // roundText가 panel의 자식이 아닌 구조여도(계층에 안 의존) 확실히 같이 뜨게
        pendingTcs = tcs;
        hideRoutine = StartCoroutine(HideAfterDelay(tcs));

        return tcs.Task;
    }

    private IEnumerator HideAfterDelay(TaskCompletionSource<bool> tcs)
    {
        yield return new WaitForSeconds(displayDuration);
        if (panel != null)
            panel.SetActive(false);
        if (roundText != null)
            roundText.enabled = false; // 계층 구조와 무관하게 텍스트도 같이 사라지도록
        hideRoutine = null;
        pendingTcs = null;
        tcs.TrySetResult(true);
    }
}
}
