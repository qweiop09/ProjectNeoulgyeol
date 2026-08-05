using System;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Runtime.Battles.Decision;
using _01_Scripts.Runtime.Battles.Phase.Decision.ActionMenu;
using _01_Scripts.Runtime.Worlds.Inventory;
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

    private Action<CharacterSkill> _onActionSettingCompleted;
    private Action<Item>           _onItemActionSettingCompleted;
    private Action                 _onStayActionSettingCompleted;

    private void OnEnable()
    {
        EnsureAttackArrowController();
        EnsureTrackingCamera();

        characterChoiceController.OnCharacterSelected += HandleSelectionSignal;

        _onActionSettingCompleted = (data) =>
        {
            // 이전에 아이템을 골랐다가 스킬/공격으로 바꾸는 경우, 남아있던 예약을 먼저 풀어준다
            ReleaseIfReservedItem();

            _currentActData = new SkillActData
            {
                CastPlayerCharacter = selectedActCaster,
                UseSlot             = _currentActData?.UseSlot ?? 0,
                UseSkill            = data
            };
            ChangeSelectionState(SelectionState.SelectingActTarget);
        };

        _onItemActionSettingCompleted = (item) =>
        {
            // 같은 방식으로 이전 선택(다른 아이템 포함)의 예약부터 해제
            ReleaseIfReservedItem();

            // 장비 장착의 equippedBy와 같은 방식: 선택 시점에 바로 예약해서 같은 라운드 내 중복 사용을 막는다
            if (!InventoryManager.Instance.ReserveItem(item, 1))
            {
                Debug.LogWarning($"[ActionSelectionPhaseManager] '{item.itemName}' 예약 실패 — 이미 다른 캐릭터가 사용하기로 했습니다.");
                return;
            }

            _currentActData = new ItemActData
            {
                CastPlayerCharacter = selectedActCaster,
                UseSlot             = _currentActData?.UseSlot ?? 0,
                UseItem             = item
            };
            ChangeSelectionState(SelectionState.SelectingActTarget);
        };

        _onStayActionSettingCompleted = () =>
        {
            int slot = _currentActData?.UseSlot ?? 0;
            ReleaseIfReservedItem();

            StayActData stayData = new StayActData(selectedActCaster, slot);

            attackArrowController?.HideTrackingArrow();
            CompleteActSelected?.Invoke(stayData);
            selectedActTarget = null;
            _currentActData = null;

            ChangeSelectionState(SelectionState.SelectingActCaster);
        };

        characterActionUIController.CompletedActionSetting     += _onActionSettingCompleted;
        characterActionUIController.CompletedItemActionSetting += _onItemActionSettingCompleted;
        characterActionUIController.CompletedStayActionSetting += _onStayActionSettingCompleted;
    }

    private void OnDisable()
    {
        characterChoiceController.OnCharacterSelected -= HandleSelectionSignal;

        if (characterActionUIController != null)
        {
            characterActionUIController.CompletedActionSetting     -= _onActionSettingCompleted;
            characterActionUIController.CompletedItemActionSetting -= _onItemActionSettingCompleted;
            characterActionUIController.CompletedStayActionSetting -= _onStayActionSettingCompleted;
        }

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
        ClearManager();
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

        ReleaseIfReservedItem();

        currentState = SelectionState.Stay;
        selectedActTarget = null;
        selectedActCaster = null;
        _currentActData = null;
    }

    // 확정(CompleteActSelected 호출)되지 않은 채로 아이템 선택이 버려질 때 예약을 되돌린다
    private void ReleaseIfReservedItem()
    {
        if (_currentActData is ItemActData itemActData)
            InventoryManager.Instance.ReleaseReservation(itemActData.UseItem, 1);
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

                ReleaseIfReservedItem();
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
            // 타겟 선택을 취소하고 돌아온 경우 포함 — 스킬/공격과 마찬가지로 아이템도 다시 골라야 하므로
            // 매달려 있던 예약이 있다면 여기서 같이 풀어준다 (안 그러면 본인이 예약한 아이템이 본인 메뉴에서 사라져 보임).
            ReleaseIfReservedItem();
            _currentActData = null;

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