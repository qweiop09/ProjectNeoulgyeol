using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public class ParticleBehaviour : PlayableBehaviour
{
    public List<Transform> anchorTransforms; // 파티클을 띄울 위치들 (캐스터 하나, 또는 메인+추가 타겟 전원)
    public ParticleSystem particlePrefab;
    public Vector3 localOffset;

    private bool hasSpawned;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (hasSpawned || particlePrefab == null || anchorTransforms == null) return;
        hasSpawned = true;

        foreach (Transform anchor in anchorTransforms)
        {
            if (anchor == null) continue;

            // 클립 길이와 무관하게 파티클이 끝까지 자연스럽게 재생되도록, 파티클 자체 수명에 맞춰 정리 예약
            Vector3 spawnPosition = anchor.position + anchor.TransformDirection(localOffset);
            ParticleLifetimeUtility.SpawnAndAutoDestroy(particlePrefab, spawnPosition, anchor.rotation);
        }
    }
}
}
