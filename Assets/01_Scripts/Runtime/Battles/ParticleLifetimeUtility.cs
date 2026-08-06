using UnityEngine;

// Timeline 재생 여부와 무관하게 "월드 좌표에 파티클 프리팹을 Instantiate하고 자기 수명만큼 살다 자동 정리"하는
// 로직을 공용화한다. Timeline 안에서는 ParticleBehaviour가, 밖에서는(예: 버프 만료) UI 쪽이 이걸 그대로 재사용한다.
public static class ParticleLifetimeUtility
{
    // 이펙트 프리팹이 자식 ParticleSystem(연기/잔상 등)을 여러 개 겹쳐 만드는 경우가 흔해서,
    // 루트만 보지 않고 전체 중 가장 긴 수명을 기준으로 정리 시점을 잡는다.
    public static float GetTotalLifetime(ParticleSystem root)
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

    // prefab을 위치/회전에 Instantiate해서 재생하고, 수명이 다하면 자동으로 정리한다. prefab이 없으면 아무 것도 하지 않는다.
    public static void SpawnAndAutoDestroy(ParticleSystem prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;

        ParticleSystem instance = Object.Instantiate(prefab, position, rotation);
        instance.Play();

        Object.Destroy(instance.gameObject, GetTotalLifetime(instance));
    }
}
