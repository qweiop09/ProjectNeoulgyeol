using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public class MoveToTargetBehaviour : PlayableBehaviour
{
    public Transform targetTransform;
    public AnimationCurve easeCurve;

    private Vector3 startPosition;
    private bool isInitialized = false;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // 트랙에 바인딩된 대상 (이동시킬 캐릭터)
        Transform actor = playerData as Transform;
        if (actor == null || targetTransform == null) return;

        // 1. 재생 시작 시점에 딱 한 번 현재 위치를 저장
        if (!isInitialized)
        {
            startPosition = actor.position;
            isInitialized = true;
        }

        // 2. 현재 클립의 진행도 계산 (0 ~ 1)
        float progress = (float)(playable.GetTime() / playable.GetDuration());
        
        // 3. 커브가 있다면 커브를 적용해서 더 부드럽게 (Ease In/Out)
        float evaluatedProgress = easeCurve != null ? easeCurve.Evaluate(progress) : progress;

        // 4. 선형 보간으로 이동
        actor.position = Vector3.Lerp(startPosition, targetTransform.position, evaluatedProgress);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        isInitialized = false;
    }
}
}