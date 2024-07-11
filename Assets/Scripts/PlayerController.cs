using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    private Animator animator;
    private CharacterController characterController;
    private Vector2 movement;
    private Vector3 lookDirection;
    private bool isRunning;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // PlayerInputActions �ʱ�ȭ
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // �׼ǿ� �ݹ� ���
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Look.performed += OnLook;
        playerInputActions.Player.Look.canceled += OnLook;
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;

        // PlayerInputActions Ȱ��ȭ
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        // �׼ǿ��� �ݹ� ����
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Look.canceled -= OnLook;
        playerInputActions.Player.Run.performed -= OnRun;
        playerInputActions.Player.Run.canceled -= OnRun;

        // PlayerInputActions ��Ȱ��ȭ
        playerInputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        Vector3 mousePos = context.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            lookDirection = hitInfo.point - transform.position;
            lookDirection.y = 0; // Y�� ȸ�� ����
        }
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private void Update()
    {
        // ĳ���� �̵� ó��
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // �ִϸ����� �Ű����� ������Ʈ
        float horizontal = Vector3.Dot(moveDirection, transform.right);
        float vertical = Vector3.Dot(moveDirection, transform.forward);

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", moveDirection.magnitude); // �̵� �ӵ��� ���� ����

        // ĳ���͸� ���콺 �������� ȸ��
        RotateToMouse();
    }

    private void RotateToMouse()
    {
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
