using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles.Decision
{
public class ActionSelectionPhaseController : MonoBehaviour
{
    private enum RaycastMode
    {
        Physics2D,
        Physics3D
    }
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private RaycastMode raycastMode = RaycastMode.Physics2D;
    [SerializeField] private InputActionReference leftClickAction;
    [SerializeField] private InputActionReference pointerPositionAction;
    [SerializeField] private LayerMask characterLayerMask = ~0;
    [SerializeField] private RectTransform actionMenuPanel;
    [SerializeField] private RectTransform actionMenuPrefab;
    [SerializeField] private Canvas actionMenuCanvas;
    [SerializeField] private Vector2 menuScreenOffset = new Vector2(120f, 0f);

    private bool isActive;
    private bool enabledLeftClickActionInternally;
    private Transform selectedCharacter;
    private RectTransform activeActionMenu;

    public Transform SelectedCharacter => selectedCharacter;

    public void ActivateActionSelectionPhase()
    {
        isActive = true;
        EnableLeftClickAction();
        HideActionMenu();
    }

    public void DeactivateActionSelectionPhase()
    {
        isActive = false;
        DisableLeftClickActionIfNeeded();
        selectedCharacter = null;
        HideActionMenu();
    }

    private void Awake()
    {
        if (raycastCamera == null)
        {
            raycastCamera = Camera.main;
        }

        if (actionMenuPanel != null)
        {
            activeActionMenu = actionMenuPanel;
            activeActionMenu.gameObject.SetActive(false);

            if (actionMenuCanvas == null)
            {
                actionMenuCanvas = actionMenuPanel.GetComponentInParent<Canvas>();
            }
        }
    }

    private void OnEnable()
    {
        if (leftClickAction != null && leftClickAction.action != null)
        {
            leftClickAction.action.performed += OnLeftClickPerformed;
        }
    }

    private void OnDisable()
    {
        if (leftClickAction != null && leftClickAction.action != null)
        {
            leftClickAction.action.performed -= OnLeftClickPerformed;
        }

        DeactivateActionSelectionPhase();
    }

    private void OnLeftClickPerformed(InputAction.CallbackContext context)
    {
        if (!isActive)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        TrySelectCharacter(GetPointerScreenPosition());
    }

    private void EnableLeftClickAction()
    {
        if (leftClickAction == null || leftClickAction.action == null)
        {
            Debug.LogWarning("ActionSelectionPhaseController needs a LeftClick input action.");
            return;
        }

        if (!leftClickAction.action.enabled)
        {
            leftClickAction.action.Enable();
            enabledLeftClickActionInternally = true;
        }
    }

    private void DisableLeftClickActionIfNeeded()
    {
        if (enabledLeftClickActionInternally && leftClickAction != null && leftClickAction.action != null)
        {
            leftClickAction.action.Disable();
        }

        enabledLeftClickActionInternally = false;
    }

    private Vector2 GetPointerScreenPosition()
    {
        if (pointerPositionAction != null && pointerPositionAction.action != null)
        {
            return pointerPositionAction.action.ReadValue<Vector2>();
        }

        if (Pointer.current != null)
        {
            return Pointer.current.position.ReadValue();
        }

        return Vector2.zero;
    }

    private void TrySelectCharacter(Vector3 screenPosition)
    {
        if (raycastCamera == null)
        {
            Debug.LogWarning("ActionSelectionPhaseController needs a camera for character raycasts.");
            return;
        }

        Ray ray = raycastCamera.ScreenPointToRay(screenPosition);
        Transform hitTransform = GetRaycastHitTransform(ray);

        if (hitTransform == null)
        {
            HideActionMenu();
            return;
        }

        selectedCharacter = hitTransform;
        ShowActionMenu(selectedCharacter);
    }

    private Transform GetRaycastHitTransform(Ray ray)
    {
        if (raycastMode == RaycastMode.Physics2D)
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, characterLayerMask);
            return hit.collider != null ? hit.collider.transform : null;
        }

        if (Physics.Raycast(ray, out RaycastHit hit3D, Mathf.Infinity, characterLayerMask))
        {
            return hit3D.collider.transform;
        }

        return null;
    }

    private void ShowActionMenu(Transform characterTransform)
    {
        RectTransform menu = GetActionMenu();

        if (menu == null)
        {
            Debug.LogWarning("ActionSelectionPhaseController needs an action menu panel or prefab.");
            return;
        }

        Vector3 menuScreenPosition = raycastCamera.WorldToScreenPoint(characterTransform.position);
        menuScreenPosition += (Vector3)menuScreenOffset;

        menu.gameObject.SetActive(true);

        if (actionMenuCanvas != null)
        {
            RectTransform canvasRectTransform = actionMenuCanvas.transform as RectTransform;
            Camera canvasCamera = actionMenuCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : actionMenuCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    menuScreenPosition,
                    canvasCamera,
                    out Vector2 localPoint))
            {
                menu.anchoredPosition = localPoint;
            }

            return;
        }

        menu.position = menuScreenPosition;
    }

    private RectTransform GetActionMenu()
    {
        if (activeActionMenu != null)
        {
            return activeActionMenu;
        }

        if (actionMenuPrefab == null || actionMenuCanvas == null)
        {
            return null;
        }

        activeActionMenu = Instantiate(actionMenuPrefab, actionMenuCanvas.transform);
        activeActionMenu.gameObject.SetActive(false);

        return activeActionMenu;
    }

    private void HideActionMenu()
    {
        if (activeActionMenu != null)
        {
            activeActionMenu.gameObject.SetActive(false);
        }
    }
}
}
