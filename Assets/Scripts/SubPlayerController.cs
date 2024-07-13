using UnityEngine;
using UnityEngine.InputSystem;

/* [ Memo ]
  //0712
  - �÷��̾ �ٶ󺸴� ������ ���콺 �����ͷ� ����
  - 8���� �ִϸ��̼� ���� �⺻, �ȱ�, �ٱ�
  - �ȱ� ASDW �ٱ� Shift
  - Blend Tree ���
  - ���� �� ī�޶� �ʿ�
 */

public class SubPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float maxRunSpeed = 5.0f;                     // �ִ� �ٱ� �ӵ�
    [SerializeField] float minSpeed = 2.0f;                       // �ּ� �̵� �ӵ�
    [SerializeField] float acceleration = 2.0f;                   // ���ӵ�
    [SerializeField] float deceleration = 5.0f;                   // ���ӵ�
    [SerializeField] private float currentSpeed = 0.0f;           // ���� �ӵ� (�ν����Ϳ� ����)

    private Animator animator;                                    // ĳ���� �ִϸ�����
    private CharacterController characterController;              // ĳ���� ��Ʈ�ѷ�
    private PlayerInputActions playerInputActions;                // �Է� �ý��� �׼�

    private Vector2 movement;                                     // �̵� �Է� ��
    private Vector3 lookDirection;                                // ���콺 ��ġ ��� ȸ�� ����
    private bool isMoving = false;                                // �̵� �� ����

    private void Awake()
    {
        InitializeComponents();
        InitializeInputActions();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    // ������Ʈ �ʱ�ȭ
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    // �Է� �׼� �ʱ�ȭ
    private void InitializeInputActions()
    {
        playerInputActions = new PlayerInputActions();
    }

    // �Է� �׼� Ȱ��ȭ
    private void EnableInputActions()
    {
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Look.performed += OnLook;
        playerInputActions.Player.Look.canceled += OnLook;
        playerInputActions.Enable();
    }

    // �Է� �׼� ��Ȱ��ȭ
    private void DisableInputActions()
    {
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Look.canceled -= OnLook;
        playerInputActions.Disable();
    }

    // �̵� �Է� ó��
    private void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        isMoving = context.phase == InputActionPhase.Performed;
    }

    // ���콺 ��ġ �Է� ó��
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

    // �� �����Ӹ��� �̵� �� �ִϸ����� ������Ʈ
    private void Update()
    {
        MoveCharacter();
        UpdateAnimatorParameters();
    }

    // �� ������ �Ĺݿ� ĳ���� ȸ�� ó��
    private void LateUpdate()
    {
        RotateToMouse();
    }

    // ĳ���� �̵� ó��
    private void MoveCharacter()
    {
        if (isMoving)
        {
            // ���ӵ��� ���� �ӵ� ����, �ּ� �ӵ� �̻����� ����
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxRunSpeed); // �ּ� �� �ִ� �ӵ� ����
        }
        else
        {
            // ���ӵ��� ���� �ӵ� ����
            currentSpeed -= deceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0.0f); // �ּ� �ӵ� ����
        }

        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    // �ִϸ����� �Ķ���� ������Ʈ
    private void UpdateAnimatorParameters()
    {
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        float speedMultiplier = currentSpeed / maxRunSpeed; // ���� �ӵ� ����

        animator.SetFloat("Horizontal", localMove.x * speedMultiplier);
        animator.SetFloat("Vertical", localMove.z * speedMultiplier);
    }

    // ĳ���͸� ���콺 �������� ȸ��
    private void RotateToMouse()
    {
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
