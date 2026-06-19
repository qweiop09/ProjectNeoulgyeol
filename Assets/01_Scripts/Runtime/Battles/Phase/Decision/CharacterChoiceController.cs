using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles.Decision
{
public class CharacterChoiceController : MonoBehaviour
{
    private enum RaycastMode { Physics2D, Physics3D }

    [SerializeField] private Camera raycastCamera;
    [SerializeField] private RaycastMode raycastMode = RaycastMode.Physics2D;
    [SerializeField] private InputActionReference leftClickAction;
    [SerializeField] private InputActionReference pointerPositionAction;
    [SerializeField] private LayerMask characterLayerMask = ~0;
    
    [Space(10)]
    [Header("Internal Fields")]
    
    [SerializeField] private bool isActive;
    [SerializeField] private CharacterHandler selectedCharacter;

    private bool _pendingClick;
    private Vector2 _pendingClickPosition;

    public CharacterHandler SelectedCharacter => selectedCharacter;

    // 캐릭터가 선택됐을 때 외부에 알리는 이벤트
    public event System.Action<CharacterHandler> OnCharacterSelected;

    // 선택 페이즈 시작
    public void ActivateActionSelectionPhase()
    {
        Debug.Log("Activating Action Selection Phase");
        isActive = true;
        selectedCharacter = null;
        _pendingClick = false;
        _pendingClickPosition = Vector2.zero;
    }

    // 선택 페이즈 종료
    public void DeactivateActionSelectionPhase()
    {
        Debug.Log("Deactivating Action Selection Phase");
        isActive = false;
        selectedCharacter = null;
        _pendingClick = false;
        _pendingClickPosition = Vector2.zero;
    }
    
    
    private Camera GetCamera()
    {
        if (raycastCamera != null) return raycastCamera;
        raycastCamera = Camera.main;
        return raycastCamera;
    }

    private void OnEnable()
    {
        if (leftClickAction?.action == null) return;
        leftClickAction.action.Enable();
        leftClickAction.action.performed += OnLeftClickPerformed;
    }

    private void OnDisable()
    {
        if (leftClickAction?.action != null)
            leftClickAction.action.performed -= OnLeftClickPerformed;

        DeactivateActionSelectionPhase();
    }
    
    
    // 기능

    private void Update()
    {
        if (!_pendingClick) return;
        _pendingClick = false;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[Click] UI에 가려져 무시됨");
            return;
        }

        TrySelectCharacter(_pendingClickPosition);
    }

    // 화면 클릭 위치에서 캐릭터 선택 시도
    private void TrySelectCharacter(Vector3 screenPosition)
    {
        var cam = GetCamera();
        if (cam == null)
        {
            Debug.LogWarning("[Click] Camera.main을 찾을 수 없음");
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPosition);
        CharacterHandler hitCharacterHandler = GetRayCastHitTransform(ray);

        Debug.Log("[Click] RayCast hit: " + (hitCharacterHandler != null ? hitCharacterHandler.name : "None"));

        selectedCharacter = hitCharacterHandler;
        OnCharacterSelected?.Invoke(selectedCharacter);
    }

    // 클릭 시 캐릭터 선택 시도
    private void OnLeftClickPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"[Click] 콜백 수신 — isActive: {isActive}, action enabled: {leftClickAction?.action?.enabled}");
        if (!isActive) return;
        _pendingClick = true;
        _pendingClickPosition = GetPointerScreenPosition();
    }


    // 클릭 위치에서 레이캐스트를 쏴서 캐릭터 핸들러 가져오기
    private CharacterHandler GetRayCastHitTransform(Ray ray)
    {
        if (raycastMode == RaycastMode.Physics2D)
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, characterLayerMask);
            if (hit.collider == null) return null;
            return hit.collider.GetComponent<CharacterHandler>();
        }

        if (Physics.Raycast(ray, out RaycastHit hit3D, Mathf.Infinity, characterLayerMask))
            return hit3D.collider.GetComponent<CharacterHandler>();

        return null;
    }

    private Vector2 GetPointerScreenPosition()
    {
        if (pointerPositionAction.action != null)
            return pointerPositionAction.action.ReadValue<Vector2>();
    
        if (Pointer.current != null)
            return Pointer.current.position.ReadValue();
    
        return Vector2.zero;
    }
}
}