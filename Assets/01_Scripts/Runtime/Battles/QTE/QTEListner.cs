using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles
{
    public enum QTEResult { Perfect, Good, Bad, Fail }

    public class QTEListner : MonoBehaviour
    {
        [SerializeField] private InputActionReference qteInputAction;

        private bool isActive;
        private bool hasJudged;
        private float currentTime;
        private float duration;
        
        private float perfectTime;
        private float goodTime;
        private float badTime;

        private void OnEnable()
        {
            qteInputAction.action.Enable();
            qteInputAction.action.started += OnButtonDown;
        }

        private void OnDisable()
        {
            qteInputAction.action.started -= OnButtonDown;
            qteInputAction.action.Disable();
        }

        // Mixer가 매 프레임 호출
        public void UpdateQteState(float localTime, float clipDuration, float perfect, float good, float bad)
        {
            if (!isActive)
            {
                isActive = true;
                hasJudged = false;
            }

            currentTime = localTime;
            duration = clipDuration;
            perfectTime = perfect;
            goodTime = good;
            badTime = bad;

            if (!hasJudged && currentTime >= duration)
            {
                hasJudged = true;
                QTECoordinator.Instance.OnQTEMarkerReceived?.Invoke(QTEResult.Fail);
            }
            
        }

        private QTEResult JudgeByTime(float time)
        {
            float center = duration / 2f;
            float diff = Mathf.Abs(time - center);

            if (diff <= perfectTime) return QTEResult.Perfect;
            if (diff <= goodTime)    return QTEResult.Good;
            if (diff <= badTime)     return QTEResult.Bad;
            return QTEResult.Fail;
        }

        // Mixer가 클립이 끝났을 때 호출
        public void ClearQteState()
        {
            isActive = false;
        }

        private void OnButtonDown(InputAction.CallbackContext ctx)
        {
            Debug.Log("QTE Button Pressed at: " + currentTime + " seconds");
            
            if (!isActive || hasJudged) return;
            
            Debug.Log("QTE Judging at: " + currentTime + " seconds");

            hasJudged = true;
            QTECoordinator.Instance.OnQTEMarkerReceived?.Invoke(JudgeByTime(currentTime));
        }

    }
}