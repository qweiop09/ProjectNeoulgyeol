using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles
{
    // Hit: 못 막음/놓침 (Bad와 동일 판정이라 통합됨)
    public enum QTEResult { Perfect, Good, Hit }

    // 판정별(Perfect/Good/Hit) 데미지 배율. 아군 캐스터 기준값 하나만 가진다 —
    // 적군이 캐스터일 때는 DamageCalculation에서 1.0 기준 대칭 반전(2.0 - 값)해서 사용한다.
    [Serializable]
    public struct QteMultiplierSet
    {
        public float perfect;
        public float good;
        public float hit;

        public QteMultiplierSet(float perfect, float good, float hit)
        {
            this.perfect = perfect;
            this.good = good;
            this.hit = hit;
        }
    }

    // QTE 판정 결과 + 해당 히트의 데미지 계수/배율 (QteClip에서 이관됨)
    public readonly struct QTEHitInfo
    {
        public readonly QTEResult Result;
        public readonly float HpDamageCoefficient;
        public readonly float StaminaDamageCoefficient;
        public readonly QteMultiplierSet Multipliers;

        public QTEHitInfo(QTEResult result, float hpDamageCoefficient, float staminaDamageCoefficient,
            QteMultiplierSet multipliers)
        {
            Result = result;
            HpDamageCoefficient = hpDamageCoefficient;
            StaminaDamageCoefficient = staminaDamageCoefficient;
            Multipliers = multipliers;
        }
    }

    public class QTEListner : MonoBehaviour
    {
        [SerializeField] private InputActionReference qteInputAction;

        private bool isActive;
        private bool stayPressed;
        private bool hasJudged;
        private float currentTime;
        private float duration;

        private bool isJudged;
        
        private float perfectTime;
        private float goodTime;
        private float badTime;

        private float hpDamageCoefficient;
        private float staminaDamageCoefficient;
        private QteMultiplierSet multipliers;

        private void OnEnable()
        {
            isJudged = false;
            hasJudged = false;
            
            qteInputAction.action.Enable();
            qteInputAction.action.started += OnButtonDown;
            qteInputAction.action.canceled += OnButtonUp;
        }

        private void OnDisable()
        {
            qteInputAction.action.started -= OnButtonDown;
            qteInputAction.action.canceled -= OnButtonUp;
            qteInputAction.action.Disable();
        }

        // Mixer가 매 프레임 호출
        public void UpdateQteState(float localTime, float clipDuration, float perfect, float good, float bad,
            float hpCoefficient, float staminaCoefficient, QteMultiplierSet multiplierSet)
        {
            if (!isActive)
            {
                isActive = true;
            }

            currentTime = localTime;
            duration = clipDuration;
            perfectTime = perfect;
            goodTime = good;
            badTime = bad;
            hpDamageCoefficient = hpCoefficient;
            staminaDamageCoefficient = staminaCoefficient;
            multipliers = multiplierSet;
        }

        private QTEResult JudgeByTime(float time)
        {
            isJudged = true;

            float center = duration / 2f;
            float diff = Mathf.Abs(time - center);

            if (diff <= perfectTime) return QTEResult.Perfect;
            if (diff <= goodTime)    return QTEResult.Good;
            return QTEResult.Hit;
        }

        // Mixer가 클립이 끝났을 때 호출
        public void ClearQteState()
        {
            if (!hasJudged && isActive)
                QTECoordinator.Instance.OnQTEMarkerReceived?.Invoke(
                    new QTEHitInfo(QTEResult.Hit, hpDamageCoefficient, staminaDamageCoefficient, multipliers));

            hasJudged = false;
            isActive = false;
        }

        private void OnButtonDown(InputAction.CallbackContext ctx)
        {
            if (!isActive || stayPressed)
                return;

            hasJudged = true;
            stayPressed = true;
            QTECoordinator.Instance.OnQTEMarkerReceived?.Invoke(
                new QTEHitInfo(JudgeByTime(currentTime), hpDamageCoefficient, staminaDamageCoefficient, multipliers));
        }

        
        private void OnButtonUp(InputAction.CallbackContext ctx)
        {
            if (!isActive)
                return;
            
            stayPressed = false;
        }

    }
}