using _01_Scripts.DTO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles.Decision
{
public class ActionSelectionPhaseManager : MonoBehaviour
{
    private enum SelectionState
     {
         Idle,
         SelectingAction,
         SelectingTarget
     }

    private bool isDetecting = false;
    
     private SelectionState currentState = SelectionState.Idle;
    
    [SerializeField] private CharacterChoiceController characterChoiceContoller;
    [SerializeField] private ActionSettingController actionSettingController;
    [SerializeField] private ChracterActionUI characterActionUI;
    
    private void OnEnable()
    {
        characterChoiceContoller.OnCharacterSelected += SpreadSelectedSignal;
        characterChoiceContoller.OnSelectionCleared += HandleSelectionClearedSignal;
    }

    private void OnDisable()
    {
        characterChoiceContoller.OnCharacterSelected -= SpreadSelectedSignal;
        characterChoiceContoller.OnSelectionCleared -= HandleSelectionClearedSignal;
    }
    
    private void SpreadSelectedSignal(CharacterHandler characterHandler)
     {
         characterChoiceContoller.DeactivateActionSelectionPhase();
         
         characterActionUI.HandleCharacterSelected(characterHandler);
     }
    
    private void HandleSelectionClearedSignal()
    {
        characterActionUI.HandleSelectionCleared();
    }

    // 매니저 기능 시작점
    // 액션 선택 단계(ray기반 클릭) 활성화
    public void ActivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Activated");
        
        characterChoiceContoller.ActivateActionSelectionPhase();
        currentState = SelectionState.SelectingTarget;
        isDetecting = true;

        // 액션 선택 UI 활성화
    }

    // 액션 선택 단계(ray기반 클릭) 비활성화
    public void DeactivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Deactivated");
        
        characterChoiceContoller.DeactivateActionSelectionPhase();
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