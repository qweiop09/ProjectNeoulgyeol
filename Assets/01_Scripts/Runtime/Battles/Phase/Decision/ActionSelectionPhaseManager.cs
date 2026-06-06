using System;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Runtime.Battles.Decision;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    // [SerializeField] private Camera trackingCamera;
    
    public event Action<ActData> CompleteActSelected;
    
    [Space(10)]
    [Header("Internal Fields")]
    
    [SerializeField] private CharacterHandler selectedActCaster;
    [SerializeField] private CharacterHandler selectedActTarget;
    
    private ActData _currentActData;
    private void OnEnable()
    {
        // EnsureAttackArrowController();
        // EnsureTrackingCamera();
        
        characterChoiceController.OnCharacterSelected += HandleSelectionSignal;
        
        // 설정된 행동 받아와서 반영하기
        // 객체의 수명이 길기 때문에 굳이 구독취소 안함(메서드로 빼기 귀찮)
        characterActionUIController.CompletedActionSetting +=
            (data) =>
            {
                // selectedActCaster.GetCharacterBattleData().TargetingData[0] = data.characterBattleData.TargetingData[0];
                // OnActSelected?.Invoke(_currentActData);
                
                _currentActData.UseSkill = data;
                ChangeSelectionState(SelectionState.SelectingActTarget);
            };
    }
    
    private void OnDisable()
    {
        characterChoiceController.OnCharacterSelected -= HandleSelectionSignal;
        attackArrowController?.HideTrackingArrow();
    }
    
    // private void LateUpdate()
    // {
    //     if (currentState != SelectionState.SelectingActTarget || selectedActCharacter == null)
    //     {
    //         attackArrowController?.HideTrackingArrow();
    //         return;
    //     }
    //
    //     attackArrowController?.ShowTrackingArrow(selectedActCharacter, GetPointerWorldPosition());
    // }
    
    
    // 매니저 기능 시작점
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
    }

    private void SelectActCaster()
    {
        currentState = SelectionState.SelectingActCaster;
        
        ActivateCharacterSelectionPhase();
    }
    
    // 액션 선택 단계(ray기반 클릭) 활성화
    public void ActivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Activated");
        
        characterChoiceController.ActivateActionSelectionPhase();
        ChangeSelectionState(SelectionState.SelectingActCaster);
    }

    // 액션 선택 단계(ray기반 클릭) 비활성화
    public void DeactivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Deactivated");
        
        characterChoiceController.DeactivateActionSelectionPhase();
        ChangeSelectionState(SelectionState.Stay);
    }
    
    // 클릭한 객체 받기
    private void HandleSelectionSignal(CharacterHandler characterHandler)
     {
         if (characterHandler == null)
         {
             Debug.Log("Null character handler received, ignoring.");
             if (currentState == SelectionState.Stay) return;
             if (currentState == SelectionState.SelectingActCaster ) return;
             
             ChangeSelectionState(currentState - 1 );   
         }
         Debug.Log("Character Selected: " + characterHandler.name + " in state: " + currentState);
         
         // 현재 상태에 따른 전파 위치 선별
         if ( currentState == SelectionState.SelectingActCaster )
         {
             if (characterHandler.characterType == CharacterHandler.CharacterType.Enemy)
                 return;
             
             Debug.Log("Setting selected act caster: " + characterHandler.name);
             selectedActCaster = characterHandler;
             
             _currentActData = new ActData();
             _currentActData.CastPlayerCharacter = characterHandler;
             _currentActData.UseSlot = 0; // TODO: 슬롯 선택 기능 추가되면 바꿔야할듯
             
             ChangeSelectionState(SelectionState.SelectingAction);
         }
         else if ( currentState == SelectionState.SelectingActTarget )
         {
             selectedActTarget = characterHandler;
             
             _currentActData.TargetPlayerCharacter = characterHandler;
             
             // attackArrowController?.ShowFixedArrow(selectedActCaster, selectedActTarget);
             // attackArrowController?.HideTrackingArrow();

             _currentActData.TargetSlot = 0; // TODO: 슬롯 선택 기능 추가되면 바꿔야할듯
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
        // 행동선택에서 다른 상태로 바뀔 때 UI 초기화
        if (currentState == SelectionState.SelectingAction && newState != SelectionState.SelectingAction)
        {
            await CameraHandler.Instance.PositionResetToLerp();
            
            // 이전으로 돌아가면 선택된 행동 캐릭터 초기화
            if (currentState == newState + 1)
            {
                Debug.Log("Clearing selected act character: " + selectedActCaster.name);
                characterActionUIController.HandleSelectionCleared();
                
                // attackArrowController?.HideFixedArrow(selectedActCharacter);
                // attackArrowController?.HideTrackingArrow();
                selectedActCaster = null;
            }
            else
                characterActionUIController.HideMenu();
    
        }
            
        if (currentState == newState) return;
        
        Debug.Log("Changing Selection State: " + currentState + " -> " + newState);
        
        currentState = newState;
        if (currentState == SelectionState.Stay)
        {
            // pass
        }
        else if (currentState == SelectionState.SelectingActCaster)
        {
            // pass
        }
        else if (currentState == SelectionState.SelectingAction)
        {
            characterActionUIController.HandleCharacterSelected(_currentActData);
    
            await CameraHandler.Instance.MoveToLerp(
                selectedActCaster.transform.position + new Vector3(1f, 0, -10), 1);
        }
        else if (currentState == SelectionState.SelectingActTarget)
        {
            // pass
        }
    }
    
    // 화살표 관련 코드들
    // public void SetAttackArrowsVisible(bool visible)
    // {
    //     attackArrowController?.SetFixedArrowsVisible(visible);
    // }
    //
    // public void ShowAttackArrows()
    // {
    //     attackArrowController?.ShowFixedArrows();
    // }
    //
    // public void HideAttackArrows()
    // {
    //     attackArrowController?.HideFixedArrows();
    // }
    //
    // public void ToggleAttackArrows()
    // {
    //     attackArrowController?.ToggleFixedArrows();
    // }
    //
    // private void EnsureAttackArrowController()
    // {
    //     if (attackArrowController != null)
    //         return;
    //
    //     attackArrowController = FindFirstObjectByType<AttackArrowController>();
    //
    //     if (attackArrowController != null)
    //         return;
    //
    //     GameObject attackArrowControllerObject = new GameObject("AttackArrowController");
    //     attackArrowController = attackArrowControllerObject.AddComponent<AttackArrowController>();
    // }

    
    // private void EnsureTrackingCamera()
    // {
    //     if (trackingCamera == null)
    //         trackingCamera = Camera.main;
    // }
    
    // private Vector3 GetPointerWorldPosition()
    // {
    //     EnsureTrackingCamera();
    //
    //     if (trackingCamera == null)
    //         return selectedActCharacter.transform.position;
    //
    //     Vector2 pointerScreenPosition = Pointer.current != null
    //         ? Pointer.current.position.ReadValue()
    //         : Vector2.zero;
    //
    //     Vector3 screenPosition = new Vector3(
    //         pointerScreenPosition.x,
    //         pointerScreenPosition.y,
    //         Mathf.Abs(trackingCamera.transform.position.z - selectedActCharacter.transform.position.z));
    //
    //     return trackingCamera.ScreenToWorldPoint(screenPosition);
    // }

    
}
}
