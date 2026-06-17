// 실행 담당

using System.Threading.Tasks;
using _01_Scripts.Runtime.Battles;
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

    public void Play(CharacterHandler castCharacterHandler, TimelineAsset timelineAsset, ITimelineBinder binder, SkillActData data)
    {
        if (timelineAsset == null)
        {
            Debug.LogError("TimelineDirector: TimelineAsset이 비어있습니다.");
            return;
        }
        
        castCharacterHandler.director = director; // PlayAsync를 호출하는 캐릭터에 Director 할당
        castCharacterHandler.director.playableAsset = timelineAsset; // PlayAsync를 호출하는 캐릭터의 Director에 TimelineAsset 할당
                
        director.playableAsset = timelineAsset;
        if (data != null)
            binder?.Bind(director, data);
        else
            Debug.LogWarning("TimelineDirector: ActData가 비어있습니다.");

        Debug.Log("TimelineDirector: 타임라인 재생 시작");
        director.Play();
    }

    // 기다려야 할 때 이걸 사용
    public Task PlayAsync(CharacterHandler castCharacterHandler , TimelineAsset timelineAsset, ITimelineBinder binder, SkillActData data)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        void OnStoppedHandler()
        {
            OnStopped -= OnStoppedHandler;
            tcs.SetResult(true);
        }

        OnStopped += OnStoppedHandler;
        Play(castCharacterHandler, timelineAsset, binder, data);

        return tcs.Task;
    }
}}