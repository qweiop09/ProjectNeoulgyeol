using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.8f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;

    private Rigidbody2D rb;

    private Vector2 moveDirection;
    private bool isRunning;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void OnEnable()
    {
        moveAction.action.Enable();
        runAction.action.Enable();

        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;

        runAction.action.performed += OnRunStarted;
        runAction.action.canceled += OnRunCanceled;
    }


    private void OnDisable()
    {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;

        runAction.action.performed -= OnRunStarted;
        runAction.action.canceled -= OnRunCanceled;

        moveAction.action.Disable();
        runAction.action.Disable();

        moveDirection = Vector2.zero;
        isRunning = false;
        rb.linearVelocity = Vector2.zero;
    }


    private void FixedUpdate()
    {
        Move();
    }


    private void Move()
    {
        float currentSpeed = moveSpeed;

        if (isRunning)
        {
            currentSpeed *= runMultiplier;
        }

        rb.linearVelocity = moveDirection * currentSpeed;
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }


    private void OnRunStarted(InputAction.CallbackContext context)
    {
        isRunning = true;
    }


    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        isRunning = false;
    }
}