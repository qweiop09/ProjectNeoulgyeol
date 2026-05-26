// 실행 담당

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline
{
public class TimelineDirector : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    private void Awake()
    {
        director.stopped += _ => OnStopped?.Invoke();
    }

    public event System.Action OnStopped;

    public void SetDirecter(PlayableDirector _director)
    {
        director = _director;
    }

    public void Play(TimelineAsset timelineAsset, ITimelineBinder binder, ActData data)
    {
        if (timelineAsset == null)
        {
            Debug.LogError("TimelineDirector: TimelineAsset이 비어있습니다.");
            return;
        }

        director.playableAsset = timelineAsset;
        binder?.Bind(director, data);
        director.Play();
    }

    // 기다려야 할 때 이걸 사용
    public Task PlayAsync(TimelineAsset timelineAsset, ITimelineBinder binder, ActData data)
    {
        var tcs = new TaskCompletionSource<bool>();

        void OnStoppedHandler()
        {
            OnStopped -= OnStoppedHandler;
            tcs.SetResult(true);
        }

        OnStopped += OnStoppedHandler;
        Play(timelineAsset, binder, data);

        return tcs.Task;
    }
}}