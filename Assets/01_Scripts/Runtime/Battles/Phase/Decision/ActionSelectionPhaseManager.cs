using System;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Runtime.Battles.Decision;
using _01_Scripts.Runtime.Battles.Phase.Decision.ActionMenu;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles.Phase.Decision
{
public class ActionSelectionPhaseManager : MonoBehaviour
{
    private enum SelectionState
    {
        Stay,
        SelectingActCaster,
        SelectingAction,
        SelectingActTarget
    }

    [SerializeField] private SelectionState currentState = SelectionState.Stay;

    [SerializeField] private CharacterChoiceController characterChoiceController;
    [SerializeField] private CharacterActionUIController characterActionUIController;
    [SerializeField] private AttackArrowController attackArrowController;
    [SerializeField] private Camera trackingCamera;

    public event Action<ActData> CompleteActSelected;

    [Space(10)]
    [Header("Internal Fields")]

    [SerializeField] private CharacterHandler selectedActCaster;
    [SerializeField] private CharacterHandler selectedActTarget;

    private ActData _currentActData;

    private void OnEnable()
    {
        EnsureAttackArrowController();
        EnsureTrackingCamera();

        characterChoiceController.OnCharacterSelected += HandleSelectionSignal;

        characterActionUIController.CompletedActionSetting +=
            (data) =>
            {
                _currentActData = new SkillActData
                {
                    CastPlayerCharacter = selectedActCaster,
                    UseSlot             = _currentActData?.UseSlot ?? 0,
                    UseSkill            = data
                };
                ChangeSelectionState(SelectionState.SelectingActTarget);
            };
    }

    private void OnDisable()
    {
        characterChoiceController.OnCharacterSelected -= HandleSelectionSignal;
        attackArrowController?.HideTrackingArrow();
    }

    private void LateUpdate()
    {
        if (currentState != SelectionState.SelectingActTarget || selectedActCaster == null)
        {
            attackArrowController?.HideTrackingArrow();
            return;
        }

        // [변경] 트래킹 시작 시 해당 슬롯의 기존 고정 화살표 숨김
        attackArrowController?.HideFixedArrow(selectedActCaster, _currentActData?.UseSlot ?? 0);
        attackArrowController?.ShowTrackingArrow(selectedActCaster, GetPointerWorldPosition());
    }

    public void StartActionSelectionPhase()
    {
        Debug.Log("Starting Action Selection Phase");
        SelectActCaster();
    }

    public void EndActionSelectionPhase()
    {
        Debug.Log("Ending Action Selection Phase");
        attackArrowController?.ClearAll();
        ClearManager();
    }

    private void ClearManager()
    {
        DeactivateCharacterSelectionPhase();

        currentState = SelectionState.Stay;
        selectedActTarget = null;
        selectedActCaster = null;
        _currentActData = null;
    }

    private void SelectActCaster()
    {
        ActivateCharacterSelectionPhase();
        ChangeSelectionState(SelectionState.SelectingActCaster);
    }

    public void ActivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Activated");
        characterChoiceController.ActivateActionSelectionPhase();
        ChangeSelectionState(SelectionState.SelectingActCaster);
    }

    public void DeactivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Deactivated");
        characterChoiceController.DeactivateActionSelectionPhase();
        ChangeSelectionState(SelectionState.Stay);
    }

    private void HandleSelectionSignal(CharacterHandler characterHandler)
    {
        // [변경] null 수신 시 return 누락 버그 수정 — 기존엔 null 체크 후에도 아래 코드가 실행됐음
        if (characterHandler == null)
        {
            Debug.Log("Null character handler received, ignoring.");
            if (currentState == SelectionState.Stay) return;
            if (currentState == SelectionState.SelectingActCaster) return;

            ChangeSelectionState(currentState - 1);
            return; // 추가
        }

        Debug.Log("Character Selected: " + characterHandler.name + " in state: " + currentState);

        if (currentState == SelectionState.SelectingActCaster)
        {
            if (characterHandler.characterType == CharacterHandler.CharacterType.Enemy) return;

            Debug.Log("Setting selected act caster: " + characterHandler.name);
            selectedActCaster = characterHandler;
            ChangeSelectionState(SelectionState.SelectingAction);
        }
        else if (currentState == SelectionState.SelectingActTarget)
        {
            selectedActTarget = characterHandler;

            _currentActData.TargetPlayerCharacter = characterHandler;
            _currentActData.TargetSlot = 0; // TODO: 슬롯 선택 기능 추가되면 변경

            // ShowFixedArrow — 슬롯 인덱스 기반으로 저장, 기존과 동일
            attackArrowController?.ShowFixedArrow(selectedActCaster, selectedActTarget, _currentActData.UseSlot);
            attackArrowController?.HideTrackingArrow();

            CompleteActSelected?.Invoke(_currentActData);

            selectedActCaster = null;
            selectedActTarget = null;
            _currentActData = null;

            ChangeSelectionState(SelectionState.SelectingActCaster);
        }
        else
        {
            Debug.Log("Character selected in ??? state, ignoring: "
                      + characterHandler.name + "\n Current State: " + currentState);
        }
    }

    private async void ChangeSelectionState(SelectionState newState)
    {
        if (currentState == SelectionState.SelectingAction && newState != SelectionState.SelectingAction)
        {
            if (currentState == newState + 1)
            {
                Debug.Log("Clearing selected act character: " + selectedActCaster.name);
                characterActionUIController.HandleSelectionCleared();

                attackArrowController?.HideTrackingArrow();

                _currentActData = null;
                selectedActCaster = null;
            }
            else
            {
                characterActionUIController.HideMenu();
            }

            // [변경] await 전에 상태 먼저 변경 → LateUpdate가 즉시 트래킹 화살표 시작
            currentState = newState;
        
            await CameraHandler.Instance.PositionResetToLerp();
            return;
        }

        if (currentState == newState) return;

        Debug.Log("Changing Selection State: " + currentState + " -> " + newState);

        currentState = newState;

        if (currentState == SelectionState.SelectingAction)
        {
            characterActionUIController.HandleCharacterSelected(selectedActCaster);
            await CameraHandler.Instance.MoveToLerp(
                selectedActCaster.transform.position + new Vector3(1.2f, 0.2f, -10), 1.2f);
        }
    }
    public void SetAttackArrowsVisible(bool visible) => attackArrowController?.SetFixedArrowsVisible(visible);
    public void ShowAttackArrows() => attackArrowController?.ShowFixedArrows();
    public void HideAttackArrows() => attackArrowController?.HideFixedArrows();
    public void ToggleAttackArrows() => attackArrowController?.ToggleFixedArrows();

    private void EnsureAttackArrowController()
    {
        if (attackArrowController != null) return;

        attackArrowController = FindFirstObjectByType<AttackArrowController>();

        if (attackArrowController != null) return;

        GameObject attackArrowControllerObject = new GameObject("AttackArrowController");
        attackArrowController = attackArrowControllerObject.AddComponent<AttackArrowController>();
    }

    private void EnsureTrackingCamera()
    {
        if (trackingCamera == null)
            trackingCamera = Camera.main;
    }

    private Vector3 GetPointerWorldPosition()
    {
        EnsureTrackingCamera();
    
        if (trackingCamera == null)
            return selectedActCaster.transform.position;
    
        Vector2 pointerScreenPosition = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;
    
        Vector3 screenPosition = new Vector3(
            pointerScreenPosition.x,
            pointerScreenPosition.y,
            Mathf.Abs(trackingCamera.transform.position.z - selectedActCaster.transform.position.z));
    
        return trackingCamera.ScreenToWorldPoint(screenPosition);
    }
}
}