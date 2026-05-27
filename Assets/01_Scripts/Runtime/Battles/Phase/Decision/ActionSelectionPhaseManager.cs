using System;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles.CameraControlle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _01_Scripts.Runtime.Battles.Decision
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
    [FormerlySerializedAs("commandMenuController")] [SerializeField] private ActionBuilder actionBuilder;
    [FormerlySerializedAs("characterActionUI")] [SerializeField] private CharacterActionUIController characterActionUIController;
    
    public event Action<ActData> OnActSelected;
    
    [Space(10)]
    [Header("Internal Fields")]
    
    [SerializeField] private CharacterHandler selectedActCharacter;
    [SerializeField] private CharacterHandler selectedActTarget;
    
    private void OnEnable()
    {
        characterChoiceController.OnCharacterSelected += SpreadSelectedSignal;
        characterChoiceController.OnSelectionCleared += HandleSelectionClearedSignal;
        
        // 객체의 수명이 길기 때문에 굳이 구독취소 안함(메서드로 빼기 귀찮)
        characterActionUIController.CompletedActionSetting +=
            () => { ChangeSelectionState(SelectionState.SelectingActTarget); };
    }

    private void OnDisable()
    {
        characterChoiceController.OnCharacterSelected -= SpreadSelectedSignal;
        characterChoiceController.OnSelectionCleared -= HandleSelectionClearedSignal;
    }
    
    private async void ChangeSelectionState(SelectionState newState)
    {
        // 행동선택에서 다른 상태로 바뀔 때 UI 초기화
        if (currentState == SelectionState.SelectingAction && newState != SelectionState.SelectingAction)
        {
            characterActionUIController.HandleSelectionCleared();
            await CameraHandler.Instance.PositionResetToLerp();
            
            // 이전으로 돌아가면 선택된 행동 캐릭터 초기화
            if (currentState == newState + 1)
            {
                Debug.Log("Clearing selected act character: " + selectedActCharacter.name);
                selectedActCharacter = null;
            }
            
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
    
    private void SpreadSelectedSignal(CharacterHandler characterHandler)
     {
            Debug.Log("Character Selected: " + characterHandler.name + " in state: " + currentState);
            
         // 현재 상태에 따른 전파 위치 선별
         if ( currentState == SelectionState.SelectingActCharacter )
         {
             if (characterHandler.characterType == CharacterHandler.CharacterType.Enemy)
                 return;
             
             selectedActCharacter = characterHandler;
             ChangeSelectionState(SelectionState.SelectingAction);
         }
         else if ( currentState == SelectionState.SelectingActTarget )
         {
             selectedActTarget = characterHandler;
             ChangeSelectionState(SelectionState.SelectingActCharacter);
             
             SetAct(BuildAct());
         }
         else
         {
             Debug.Log("Character selected in ??? state, ignoring: "
                       + characterHandler.name + "\n Current State: " + currentState);
         }
         
     }
    
    private void HandleSelectionClearedSignal()
    {
        if (currentState == SelectionState.Idle) return;
        if (currentState == SelectionState.SelectingActCharacter ) return;
        
        ChangeSelectionState(currentState - 1 );
    }

    private ActData BuildAct()
    {
        Debug.Log("Building ActData with Character: " + selectedActCharacter.name + " and Target: " + selectedActTarget.name);
        
        ActData tempActData
            = actionBuilder.BuildAction(selectedActCharacter, 0, 0, selectedActTarget, 0);
        
        selectedActCharacter = null;
        selectedActTarget = null;

        return
            tempActData;
    }
    
    private void SetAct(ActData actData)
    {
        OnActSelected?.Invoke(actData);
    }
    
    // 매니저 기능 시작점
    // 액션 선택 단계(ray기반 클릭) 활성화
    public void ActivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Activated");
        
        characterChoiceController.ActivateActionSelectionPhase();
        ChangeSelectionState(SelectionState.SelectingActCharacter);

        // 액션 선택 UI 활성화
    }

    // 액션 선택 단계(ray기반 클릭) 비활성화
    public void DeactivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Deactivated");
        
        ChangeSelectionState(SelectionState.Idle);
        
        // 액션 선택 UI 비활성화
    }
    
    // 행동선택 기능 시작점
    // UI기반의 행동 선택
    private void ActivateActionSelectionUI(CharacterHandler characterHandler)
    {
        Debug.Log("Activating Action Selection UI for: " + characterHandler.name);
        ChangeSelectionState(SelectionState.SelectingAction);
        
        // 선택된 캐릭터에 대한 액션 메뉴 표시
    }
    
    private void DeactivateActionSelectionUI()
    {
        Debug.Log("Deactivating Action Selection UI");
        ChangeSelectionState(SelectionState.Idle);
        
    }
    
}
}