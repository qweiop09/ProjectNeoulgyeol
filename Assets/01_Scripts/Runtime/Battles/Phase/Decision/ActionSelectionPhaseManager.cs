using System;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Runtime.Battles.Decision;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles.Phase.Decision
{
public class ActionSelectionPhaseManager : MonoBehaviour
{
    private enum SelectionState
     {
         Idle,
         SelectingActCharacter,
         SelectingAction,
         SelectingActTarget
     }
    
    [SerializeField] private SelectionState currentState = SelectionState.Idle;
    
    [SerializeField] private CharacterChoiceController characterChoiceController;
    [SerializeField] private CharacterActionUIController characterActionUIController;
    [SerializeField] private AttackArrowController attackArrowController;
    [SerializeField] private Camera trackingCamera;
    
    public event Action<CharacterHandler> OnActSelected;
    
    [Space(10)]
    [Header("Internal Fields")]
    
    [SerializeField] private CharacterHandler selectedActCharacter;
    [SerializeField] private CharacterHandler selectedActTarget;
    
    private void OnEnable()
    {
        EnsureAttackArrowController();
        EnsureTrackingCamera();
        
        characterChoiceController.OnCharacterSelected += HandleSelectionSignal;
        characterChoiceController.OnSelectionCleared += HandleSelectionClearedSignal;
        
        // 설정된 행동 받아와서 반영하기
        // 객체의 수명이 길기 때문에 굳이 구독취소 안함(메서드로 빼기 귀찮)
        characterActionUIController.CompletedActionSetting +=
            (data) =>
            {
                selectedActCharacter.GetCharacterBattleData().TargetingData[0] = data.characterBattleData.TargetingData[0];
                OnActSelected?.Invoke(selectedActCharacter);
                ChangeSelectionState(SelectionState.SelectingActTarget);
            };
    }
    
    private void OnDisable()
    {
        characterChoiceController.OnCharacterSelected -= HandleSelectionSignal;
        characterChoiceController.OnSelectionCleared -= HandleSelectionClearedSignal;
        attackArrowController?.HideTrackingArrow();
    }

    private void LateUpdate()
    {
        if (currentState != SelectionState.SelectingActTarget || selectedActCharacter == null)
        {
            attackArrowController?.HideTrackingArrow();
            return;
        }

        attackArrowController?.ShowTrackingArrow(selectedActCharacter, GetPointerWorldPosition());
    }
    
    
    // 매니저 기능 시작점
    public void StartActionSelectionPhase()
    {
        Debug.Log("Starting Action Selection Phase");
        characterChoiceController.ActivateActionSelectionPhase();
        
        ActivateCharacterSelectionPhase();
    }
    
    public void EndActionSelectionPhase()
    {
        Debug.Log("Ending Action Selection Phase");
        characterChoiceController.DeactivateActionSelectionPhase();
        attackArrowController?.ClearAll();
        
        DeactivateCharacterSelectionPhase();
    }
    
    // 액션 선택 단계(ray기반 클릭) 활성화
    public void ActivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Activated");
        
        ChangeSelectionState(SelectionState.SelectingActCharacter);
    }

    // 액션 선택 단계(ray기반 클릭) 비활성화
    public void DeactivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Deactivated");
        
        ChangeSelectionState(SelectionState.Idle);
    }
    
    private async void ChangeSelectionState(SelectionState newState)
    {
        // 행동선택에서 다른 상태로 바뀔 때 UI 초기화
        if (currentState == SelectionState.SelectingAction && newState != SelectionState.SelectingAction)
        {
            await CameraHandler.Instance.PositionResetToLerp();
            
            // 이전으로 돌아가면 선택된 행동 캐릭터 초기화
            if (currentState == newState + 1)
            {
                Debug.Log("Clearing selected act character: " + selectedActCharacter.name);
                characterActionUIController.HandleSelectionCleared();
                
                attackArrowController?.HideFixedArrow(selectedActCharacter);
                attackArrowController?.HideTrackingArrow();
                selectedActCharacter.GetCharacterBattleData().TargetingData[0] = null;
                selectedActCharacter = null;
            }
            else
                characterActionUIController.HideMenu();

        }
            
        if (currentState == newState) return;
        
        Debug.Log("Changing Selection State: " + currentState + " -> " + newState);
        
        currentState = newState;
        if (currentState == SelectionState.Idle)
        {
            // pass
        }
        else if (currentState == SelectionState.SelectingActCharacter)
        {
            // pass
        }
        else if (currentState == SelectionState.SelectingAction)
        {
            characterActionUIController.HandleCharacterSelected(selectedActCharacter);

            await CameraHandler.Instance.MoveToLerp(
                selectedActCharacter.transform.position + new Vector3(1f, 0, -10), 1);
        }
        else if (currentState == SelectionState.SelectingActTarget)
        {
            // pass
        }
    }
    
    private void HandleSelectionSignal(CharacterHandler characterHandler)
     {
         Debug.Log("Character Selected: " + characterHandler.name + " in state: " + currentState);
            
         // 현재 상태에 따른 전파 위치 선별
         if ( currentState == SelectionState.SelectingActCharacter )
         {
             if (characterHandler.characterType == CharacterHandler.CharacterType.Enemy)
                 return;
             
             selectedActCharacter = characterHandler;
             selectedActCharacter.GetCharacterBattleData()
                 .TargetingData[0] = new ActData();
             
             selectedActCharacter.GetCharacterBattleData()
                 .TargetingData[0].CastPlayerCharacter = characterHandler;
             
             ChangeSelectionState(SelectionState.SelectingAction);
         }
         else if ( currentState == SelectionState.SelectingActTarget )
         {
             selectedActTarget = characterHandler;
             
             selectedActCharacter.GetCharacterBattleData()
                 .TargetingData[0].TargetPlayerCharacter = characterHandler;
             
             attackArrowController?.ShowFixedArrow(selectedActCharacter, selectedActTarget);
             attackArrowController?.HideTrackingArrow();
             
             ChangeSelectionState(SelectionState.SelectingActCharacter);
         }
         else
         {
             Debug.Log("Character selected in ??? state, ignoring: "
                       + characterHandler.name + "\n Current State: " + currentState);
         }
         
     }
    
    private void HandleSelectionClearedSignal()
    {
        Debug.Log("Selection Cleared in state: " + currentState);
        
        if (currentState == SelectionState.Idle) return;
        if (currentState == SelectionState.SelectingActCharacter ) return;
        
        ChangeSelectionState(currentState - 1 );
    }

    public void SetAttackArrowsVisible(bool visible)
    {
        attackArrowController?.SetFixedArrowsVisible(visible);
    }

    public void ShowAttackArrows()
    {
        attackArrowController?.ShowFixedArrows();
    }

    public void HideAttackArrows()
    {
        attackArrowController?.HideFixedArrows();
    }

    public void ToggleAttackArrows()
    {
        attackArrowController?.ToggleFixedArrows();
    }

    private void EnsureAttackArrowController()
    {
        if (attackArrowController != null)
            return;

        attackArrowController = FindFirstObjectByType<AttackArrowController>();

        if (attackArrowController != null)
            return;

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
            return selectedActCharacter.transform.position;

        Vector2 pointerScreenPosition = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;

        Vector3 screenPosition = new Vector3(
            pointerScreenPosition.x,
            pointerScreenPosition.y,
            Mathf.Abs(trackingCamera.transform.position.z - selectedActCharacter.transform.position.z));

        return trackingCamera.ScreenToWorldPoint(screenPosition);
    }

    
}
}
