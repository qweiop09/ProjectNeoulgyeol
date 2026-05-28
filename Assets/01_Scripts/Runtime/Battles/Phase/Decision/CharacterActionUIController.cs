using System;
using _01_Scripts.Runtime.Battles;
using _01_Scripts.Runtime.Battles.Decision;
using Unity.VisualScripting;
using UnityEngine;
public class CharacterActionUIController : MonoBehaviour
{
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private RectTransform actionSettingPanel;
    [SerializeField] private Canvas actionSettingCanvas;
    [SerializeField] private Vector2 menuScreenOffset = new Vector2(120f, 0f);
    
    // 행동대상을 선택해야 할 때 외부에 알리는 이벤트
    public event Action CompletedAttackActionSetting;
    public event Action CompletedSkillActionSetting;
    
    private RectTransform activeActionSettingMenu;

    private void Awake()
    {
        if (actionSettingPanel != null)
        {
            activeActionSettingMenu = actionSettingPanel;
            activeActionSettingMenu.gameObject.SetActive(false);

            if (actionSettingCanvas == null)
                actionSettingCanvas = actionSettingPanel.GetComponentInParent<Canvas>();
        }
    }

    public void HandleCharacterSelected(CharacterHandler characterTransform)
    {
        ShowActionMenu(characterTransform);
    }

    public void HandleSelectionCleared()
    {
        HideActionMenu();
    }

    private void ShowActionMenu(CharacterHandler characterHandler)
    {
        Debug.Log("ShowActionMenu called for character: " + characterHandler.name);
        
        RectTransform menu = GetActionMenu();
        if (menu == null) return;

        Vector3 menuScreenPosition = raycastCamera.WorldToScreenPoint(characterHandler.transform.position);
        menuScreenPosition += (Vector3)menuScreenOffset;

        menu.gameObject.SetActive(true);

        if (actionSettingCanvas != null)
        {
            RectTransform canvasRect = actionSettingCanvas.transform as RectTransform;
            Camera canvasCamera = actionSettingCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : actionSettingCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, menuScreenPosition, canvasCamera, out Vector2 localPoint))
            {
                menu.anchoredPosition = localPoint;
            }
            return;
        }

        menu.position = menuScreenPosition;
    }

    private void HideActionMenu()
    {
        Debug.Log("HideActionMenu called");
        
        if (activeActionSettingMenu != null)
            activeActionSettingMenu.gameObject.SetActive(false);
    }

    private RectTransform GetActionMenu()
    {
        if (activeActionSettingMenu == null)
            return null;

        return activeActionSettingMenu;
    }
    
    // 버튼 클릭
    
    public void PressedAttackButton()
    {
        Debug.Log("Attack Button Pressed");
        CompletedAttackActionSetting?.Invoke();
    }

    public void PressedSkillButton()
    {
        Debug.Log("Skill Button Pressed");
        CompletedSkillActionSetting.Invoke();
    }
    
    // 아래에 UI 버튼 클릭 시 호출할 메서드 추가 (예: PressedDefendButton, PressedItemButton 등)
    
}
