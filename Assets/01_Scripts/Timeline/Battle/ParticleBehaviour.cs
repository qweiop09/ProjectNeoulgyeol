using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public class ParticleBehaviour : PlayableBehaviour
{
    public Transform anchorTransform;
    public ParticleSystem particlePrefab;
    public Vector3 localOffset;

    private bool hasSpawned;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (hasSpawned || particlePrefab == null || anchorTransform == null) return;
        hasSpawned = true;

        Vector3 spawnPosition = anchorTransform.position + anchorTransform.TransformDirection(localOffset);
        ParticleSystem instance = Object.Instantiate(particlePrefab, spawnPosition, anchorTransform.rotation);
        instance.Play();

        // 클립 길이와 무관하게 파티클이 끝까지 자연스럽게 재생되도록, 파티클 자체 수명에 맞춰 정리 예약
        Object.Destroy(instance.gameObject, GetTotalLifetime(instance));
    }

    // 이펙트 프리팹이 자식 ParticleSystem(연기/잔상 등)을 여러 개 겹쳐 만드는 경우가 흔해서,
    // 루트만 보지 않고 전체 중 가장 긴 수명을 기준으로 정리 시점을 잡는다.
    private static float GetTotalLifetime(ParticleSystem root)
    {
        float maxLifetime = 0f;
        foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>())
        {
            float psLifetime = ps.main.duration + ps.main.startLifetime.constantMax;
            if (psLifetime > maxLifetime)
                maxLifetime = psLifetime;
        }

        return maxLifetime;
    }
}
}
