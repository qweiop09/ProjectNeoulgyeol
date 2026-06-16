using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public void Initialize(int damage, Color color, DamageTextSpawner.AnimationConfig config)
    {
        _text.text = damage.ToString();
        _text.color = color;
        StartCoroutine(PlayAnimation(config));
    }

    private IEnumerator PlayAnimation(DamageTextSpawner.AnimationConfig config)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color startColor = _text.color;

        switch (config.animationType)
        {
            case DamageTextSpawner.AnimationType.FloatUp:
                yield return FloatUpRoutine(elapsed, startPos, startColor, config);
                break;

            case DamageTextSpawner.AnimationType.BounceUp:
                yield return BounceUpRoutine(startPos, startColor, config);
                break;

            case DamageTextSpawner.AnimationType.FadeOnly:
                yield return FadeOnlyRoutine(startColor, config);
                break;
        }

        Destroy(gameObject);
    }

    // 위로 서서히 올라가며 페이드아웃
    private IEnumerator FloatUpRoutine(float elapsed, Vector3 startPos, Color startColor, DamageTextSpawner.AnimationConfig config)
    {
        while (elapsed < config.lifeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / config.lifeTime;

            transform.position = startPos + Vector3.up * (config.floatDistance * t);

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            _text.color = c;

            yield return null;
        }
    }

    // 위로 튀어오르다 정점 이후 페이드아웃
    private IEnumerator BounceUpRoutine(Vector3 startPos, Color startColor, DamageTextSpawner.AnimationConfig config)
    {
        float riseTime = config.lifeTime * 0.4f;
        float holdTime = config.lifeTime * 0.1f;
        float fadeTime = config.lifeTime * 0.5f;

        // 올라가는 구간
        float elapsed = 0f;
        while (elapsed < riseTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / riseTime;
            float eased = 1f - (1f - t) * (1f - t); // ease out
            transform.position = startPos + Vector3.up * (config.floatDistance * eased);
            yield return null;
        }

        // 잠깐 유지
        elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 페이드아웃
        elapsed = 0f;
        Vector3 peakPos = transform.position;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            _text.color = c;
            transform.position = peakPos + Vector3.up * (config.floatDistance * 0.2f * t);
            yield return null;
        }
    }

    // 페이드인 → 유지 → 페이드아웃
    private IEnumerator FadeOnlyRoutine(Color startColor, DamageTextSpawner.AnimationConfig config)
    {
        float fadeInTime  = config.lifeTime * 0.2f;
        float holdTime    = config.lifeTime * 0.5f;
        float fadeOutTime = config.lifeTime * 0.3f;

        // 페이드인
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            Color c = startColor;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            _text.color = c;
            yield return null;
        }

        // 유지
        elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 페이드아웃
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            _text.color = c;
            yield return null;
        }
    }
}