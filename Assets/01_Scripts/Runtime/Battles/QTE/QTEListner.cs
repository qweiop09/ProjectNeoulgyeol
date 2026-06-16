using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles
{
    // Bad 막음,  Hit 맞음
    public enum QTEResult { Perfect, Good, Bad, Hit }

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
        public void UpdateQteState(float localTime, float clipDuration, float perfect, float good, float bad)
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
            
        }

        private QTEResult JudgeByTime(float time)
        {
            isJudged = true;
            
            float center = duration / 2f;
            float diff = Mathf.Abs(time - center);

            if (diff <= perfectTime) return QTEResult.Perfect;
            if (diff <= goodTime)    return QTEResult.Good;
            return QTEResult.Bad;
        }

        // Mixer가 클립이 끝났을 때 호출
        public void ClearQteState()
        {
            if(!hasJudged)
                if(isActive)
                    if (!stayPressed)
                        QTECoordinator.Instance.OnQTEMarkerReceived?.Invoke(QTEResult.Bad);
                    else
                        QTECoordinator.Instance.OnQTEMarkerReceived?.Invoke(QTEResult.Hit);
            
            hasJudged = false;
            isActive = false;
        }

        private void OnButtonDown(InputAction.CallbackContext ctx)
        {
            if (!isActive || stayPressed)
                return;

            hasJudged = true;
            stayPressed = true;
            QTECoordinator.Instance.OnQTEMarkerReceived?.Invoke(JudgeByTime(currentTime));
        }

        
        private void OnButtonUp(InputAction.CallbackContext ctx)
        {
            if (!isActive)
                return;
            
            stayPressed = false;
        }

    }
}