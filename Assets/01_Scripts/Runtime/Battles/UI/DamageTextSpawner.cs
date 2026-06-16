using _01_Scripts.Interfacese;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

public class DamageTextSpawner : Singleton<DamageTextSpawner>
{
    // ──────────────────────────────────────────
    // 타입 정의
    // ──────────────────────────────────────────

    public enum AnimationType
    {
        FloatUp,    // 위로 서서히 올라가며 페이드아웃
        BounceUp,   // 튀어오르고 정점에서 페이드아웃
        FadeOnly,   // 제자리에서 페이드인→페이드아웃
    }

    [System.Serializable]
    public class AnimationConfig
    {
        public AnimationType animationType = AnimationType.BounceUp;

        [Tooltip("텍스트가 살아있는 총 시간(초)")]
        public float lifeTime = 1.2f;

        [Tooltip("위로 이동하는 최대 거리 (FloatUp / BounceUp에서 사용)")]
        public float floatDistance = 1.5f;
    }

    [System.Serializable]
    public class JudgmentColorConfig
    {
        public Color perfectColor = new Color(1f, 0.84f, 0f);   // 금색
        public Color goodColor    = new Color(0.4f, 1f, 0.4f);  // 초록
        public Color badColor     = new Color(1f, 0.3f, 0.3f);  // 빨강
    }

    // ──────────────────────────────────────────
    // 인스펙터 필드
    // ──────────────────────────────────────────

    [Header("프리팹")]
    [SerializeField] private DamageText _damageTextPrefab;

    [Header("부모 캔버스")]
    [SerializeField] private Canvas _parentCanvas;

    [Header("색상 설정")]
    [SerializeField] private JudgmentColorConfig _colorConfig = new JudgmentColorConfig();

    [Header("애니메이션 설정")]
    [SerializeField] private AnimationConfig _animationConfig = new AnimationConfig();

    [Header("생성 범위 (중심점 기준 랜덤 오프셋)")]
    [SerializeField] private Vector3 _spawnRange = new Vector3(0.5f, 0f, 0f);

    // ──────────────────────────────────────────
    // 공개 API
    // ──────────────────────────────────────────

    /// <summary>
    /// 데미지 텍스트 생성
    /// </summary>
    /// <param name="worldPosition">월드 기준 생성 중심점</param>
    /// <param name="damage">표시할 데미지 값</param>
    /// <param name="judgment">QTE 판정 결과</param>
    public void SpawnDamageText(Vector3 worldPosition, int damage, QTEResult judgment)
    {
        Vector3 spawnPos = worldPosition + GetRandomOffset();
        Color color = GetColorByJudgment(judgment);

        DamageText instance = Instantiate(_damageTextPrefab, spawnPos, Quaternion.identity, _parentCanvas.transform);
        instance.Initialize(damage, color, _animationConfig);
    }

    // ──────────────────────────────────────────
    // 내부 유틸
    // ──────────────────────────────────────────

    private Vector3 GetRandomOffset()
    {
        return new Vector3(
            Random.Range(-_spawnRange.x, _spawnRange.x),
            Random.Range(-_spawnRange.y, _spawnRange.y),
            Random.Range(-_spawnRange.z, _spawnRange.z)
        );
    }

    private Color GetColorByJudgment(QTEResult judgment)
    {
        return judgment switch
        {
            QTEResult.Perfect => _colorConfig.perfectColor,
            QTEResult.Good    => _colorConfig.goodColor,
            QTEResult.Bad     => _colorConfig.badColor,
            _                 => _colorConfig.badColor,
        };
    }
}