using _01_Scripts.DTO;
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
         SelectingAction,
         SelectingActCharacter,
         SelectingActTarget
         
     }

    [SerializeField] private bool isDetecting = false;
    
    [SerializeField] private SelectionState currentState = SelectionState.Idle;
    
    [SerializeField] private CharacterChoiceController characterChoiceContoller;
    [FormerlySerializedAs("actionSettingController")] [SerializeField] private CommandMenuController commandMenuController;
    [SerializeField] private ChracterActionUI characterActionUI;
    
    private void OnEnable()
    {
        characterChoiceContoller.OnCharacterSelected += SpreadSelectedSignal;
        characterChoiceContoller.OnSelectionCleared += HandleSelectionClearedSignal;
        
        // 객체의 수명이 길기 때문에 굳이 구독취소 안함(메서드로 빼기 귀찮)
        commandMenuController.CompletedActionSetting +=
            () => { currentState = SelectionState.SelectingActTarget; };

    }

    private void OnDisable()
    {
        characterChoiceContoller.OnCharacterSelected -= SpreadSelectedSignal;
        characterChoiceContoller.OnSelectionCleared -= HandleSelectionClearedSignal;
    }
    
    private void SpreadSelectedSignal(CharacterHandler characterHandler)
     {
         // characterChoiceContoller.DeactivateActionSelectionPhase();
         
         // TODO: 현재 상태관리가 너무 하드코딩되어 있음, 이전 상태로 돌아가는 방식으로 개선 필요(스택)
         // 해당하는 메서드 두개니까 보고하기( 쿨릭시 뭐 있을 때, 뭐 없을 때 )
         // 
         
         // 현재 상태에 따른 전파 위치 선별
         if ( currentState == SelectionState.SelectingActCharacter )
         {
             currentState = SelectionState.SelectingAction;
             
             characterActionUI.HandleCharacterSelected(characterHandler);
         }
         else if ( currentState == SelectionState.SelectingActTarget )
         {
             currentState = SelectionState.SelectingAction;
             
             commandMenuController.SetTargetCharacter(characterHandler);
         }
         else
         {
             Debug.Log("Character selected in ??? state, ignoring: "
                       + characterHandler.name + "\n Current State: " + currentState);
         }
         
     }
    
    private void HandleSelectionClearedSignal()
    {
        currentState = SelectionState.SelectingActCharacter;
        
        characterActionUI.HandleSelectionCleared();
    }

    // 매니저 기능 시작점
    // 액션 선택 단계(ray기반 클릭) 활성화
    public void ActivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Activated");
        
        characterChoiceContoller.ActivateActionSelectionPhase();
        currentState = SelectionState.SelectingActCharacter;
        isDetecting = true;

        // 액션 선택 UI 활성화
    }

    // 액션 선택 단계(ray기반 클릭) 비활성화
    public void DeactivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Deactivated");
        
        // characterChoiceContoller.DeactivateActionSelectionPhase();
        currentState = SelectionState.Idle;
        isDetecting = false;
        
        // 액션 선택 UI 비활성화
    }
    
    // 행동선택 기능 시작점
    // UI기반의 행동 선택
    private void ActivateActionSelectionUI(CharacterHandler characterHandler)
    {
        Debug.Log("Activating Action Selection UI for: " + characterHandler.name);
        currentState = SelectionState.SelectingAction;
        
        // 선택된 캐릭터에 대한 액션 메뉴 표시
        characterActionUI.HandleCharacterSelected(characterHandler);
    }
    
    private void DeactivateActionSelectionUI()
    {
        Debug.Log("Deactivating Action Selection UI");
        currentState = SelectionState.Idle;
        // 액션 메뉴 숨기기
        characterActionUI.HandleSelectionCleared();
    }
    
    
    
}
}