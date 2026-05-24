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
    private bool enabledLeftClickActionInternally;
    [SerializeField] private CharacterHandler selectedCharacter;

    public CharacterHandler SelectedCharacter => selectedCharacter;

    // 캐릭터가 선택됐을 때 외부에 알리는 이벤트
    public event System.Action<CharacterHandler> OnCharacterSelected;
    // 빈 곳 클릭했을 때 (선택 해제)
    public event System.Action OnSelectionCleared;

    // 선택 페이즈 시작
    public void ActivateActionSelectionPhase()
    {
        isActive = true;
        EnableLeftClickAction();
    }

    // 선택 페이즈 종료
    public void DeactivateActionSelectionPhase()
    {
        isActive = false;
        DisableLeftClickActionIfNeeded();
        selectedCharacter = null;
    }

    private void Awake()
    {
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (leftClickAction.action != null)
            leftClickAction.action.performed += OnLeftClickPerformed;
    }

    private void OnDisable()
    {
        if (leftClickAction.action != null)
            leftClickAction.action.performed -= OnLeftClickPerformed;

        DeactivateActionSelectionPhase();
    }

    // 클릭 시 캐릭터 선택 시도
    private void OnLeftClickPerformed(InputAction.CallbackContext context)
    {
        if (!isActive) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        TrySelectCharacter(GetPointerScreenPosition());
    }

    // 화면 클릭 위치에서 캐릭터 선택 시도
    private void TrySelectCharacter(Vector3 screenPosition)
    {
        if (raycastCamera == null) return;

        Ray ray = raycastCamera.ScreenPointToRay(screenPosition);
        CharacterHandler hitCharacterHandler = GetRayCastHitTransform(ray);
        
        Debug.Log("RayCast hit: " + (hitCharacterHandler != null ? hitCharacterHandler.name : "None"));

        // 빈공간 클릭했을 때 UI지우기용 클릭 감지
        if (hitCharacterHandler == null)
        {
            Debug.Log("No character hit, clearing selection.");
            
            selectedCharacter = null;
            OnSelectionCleared?.Invoke();
            return;
        }
        // UI 레이어 클릭 감지
        if(hitCharacterHandler.transform.gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            Debug.Log("Clicked on UI, ignoring selection.");
            
            selectedCharacter = null;
            return;
        }

        selectedCharacter = hitCharacterHandler;
        OnCharacterSelected?.Invoke(selectedCharacter);
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

    // 클릭 감지 활성화
    private void EnableLeftClickAction()
    {
        if (leftClickAction.action == null) return;

        if (!leftClickAction.action.enabled)
        {
            leftClickAction.action.Enable();
            enabledLeftClickActionInternally = true;
        }
    }

    // 클릭 감지 비활성화
    private void DisableLeftClickActionIfNeeded()
    {
        if (enabledLeftClickActionInternally && leftClickAction?.action != null)
            leftClickAction.action.Disable();

        enabledLeftClickActionInternally = false;
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