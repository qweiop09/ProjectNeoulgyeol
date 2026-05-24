using _01_Scripts.Runtime.Battles;
using _01_Scripts.Runtime.Battles.Decision;
using UnityEngine;
public class ChracterActionUI : MonoBehaviour
{
    [SerializeField] private CharacterChoiceController choiceController;
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private RectTransform actionSettingPanel;
    // [SerializeField] private RectTransform actionSettingPrefab;
    [SerializeField] private Canvas actionSettingCanvas;
    [SerializeField] private Vector2 menuScreenOffset = new Vector2(120f, 0f);

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

    private void OnEnable()
    {
        if (choiceController == null) return;

        // choiceController.OnCharacterSelected += HandleCharacterSelected;
        // choiceController.OnSelectionCleared += HandleSelectionCleared;
    }

    private void OnDisable()
    {
        if (choiceController == null) return;

        // choiceController.OnCharacterSelected -= HandleCharacterSelected;
        // choiceController.OnSelectionCleared -= HandleSelectionCleared;
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

        // 프리팹 참조할 때 쓰던 코드
        // if (activeActionSettingMenu != null)
        //     return activeActionSettingMenu;
        //
        // if (actionSettingPrefab == null || actionSettingCanvas == null)
        //     return null;
        //
        // activeActionSettingMenu = Instantiate(actionSettingPrefab, actionSettingCanvas.transform);
        // activeActionSettingMenu.gameObject.SetActive(false);
        // return activeActionSettingMenu;
    }
}
