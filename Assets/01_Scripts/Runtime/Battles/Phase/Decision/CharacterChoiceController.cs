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

    // 선택 페이즈 시작
    public void ActivateActionSelectionPhase()
    {
        Debug.Log("Activating Action Selection Phase");
        
        isActive = true;
        EnableLeftClickAction();
        selectedCharacter = null;
    }

    // 선택 페이즈 종료
    public void DeactivateActionSelectionPhase()
    {
        Debug.Log("Deactivating Action Selection Phase");
        
        isActive = false;
        DisableLeftClickAction();
        selectedCharacter = null;
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
    private void DisableLeftClickAction()
    {
        if (enabledLeftClickActionInternally && leftClickAction?.action != null)
            leftClickAction.action.Disable();

        enabledLeftClickActionInternally = false;
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
    
    
    // 기능

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