using System;
using System.Collections;
using _01_Scripts.Timeline.Battle.Marker;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles
{
    public enum QTEResult
    {
        Perfect,
        Good,
        Bad,
        Fail
    }

    public class QTEListner : MonoBehaviour
    {
        public event Action<QTEResult, QTEDataMarker> OnQteCompleted;

        [SerializeField] private InputActionReference qteInputAction;

        private bool isInputButtonPressed;
        private bool hasInputInWindow;
        private float elapsedTime;
        private float inputElapsedTime;

        private void OnEnable()
        {
            qteInputAction.action.Enable();
            qteInputAction.action.performed += OnQteInput;
            qteInputAction.action.started += OnButtonDown;
            qteInputAction.action.canceled += OnButtonUp;
        }

        private void OnDisable()
        {
            qteInputAction.action.performed -= OnQteInput;
            qteInputAction.action.started -= OnButtonDown;
            qteInputAction.action.canceled -= OnButtonUp;
            qteInputAction.action.Disable();
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
        }

        private void OnButtonDown(InputAction.CallbackContext ctx) => isInputButtonPressed = true;
        private void OnButtonUp(InputAction.CallbackContext ctx) => isInputButtonPressed = false;

        private void OnQteInput(InputAction.CallbackContext ctx)
        {
            if (!hasInputInWindow)
            {
                hasInputInWindow = true;
                inputElapsedTime = elapsedTime;
            }
        }

        public void ReceiveQteSignal(QTEDataMarker data)
        {
            elapsedTime = 0f;
            hasInputInWindow = false;
            StartCoroutine(WaitAndJudge(data));
        }

        private IEnumerator WaitAndJudge(QTEDataMarker data)
        {
            yield return new WaitForSeconds(data.qteBadTime);

            QTEResult result = JudgmentQte(data);
            OnQteCompleted?.Invoke(result, data);
        }

        private QTEResult JudgmentQte(QTEDataMarker data)
        {
            if (hasInputInWindow)
            {
                if (inputElapsedTime <= data.qtePerfectTime) return QTEResult.Perfect;
                if (inputElapsedTime <= data.qteGoodTime)    return QTEResult.Good;
                return QTEResult.Bad;
            }

            return isInputButtonPressed ? QTEResult.Bad : QTEResult.Fail;
        }
    }
}