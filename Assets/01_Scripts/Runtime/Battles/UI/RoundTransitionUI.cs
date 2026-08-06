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

    public Task ShowAsync(int roundNumber)
    {
        var tcs = new TaskCompletionSource<bool>();

        if (roundText == null || panel == null)
        {
            tcs.SetResult(true); // 연출 미구성이어도 흐름은 막지 않는다 (NotificationManager와 동일한 관용구)
            return tcs.Task;
        }

        roundText.text = $"ROUND {roundNumber}";
        panel.SetActive(true);
        StartCoroutine(HideAfterDelay(tcs));

        return tcs.Task;
    }

    private IEnumerator HideAfterDelay(TaskCompletionSource<bool> tcs)
    {
        yield return new WaitForSeconds(displayDuration);
        if (panel != null)
            panel.SetActive(false);
        tcs.SetResult(true);
    }
}
}
