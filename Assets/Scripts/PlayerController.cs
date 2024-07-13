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

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float runSpeed = 5.0f;                     // �ٱ� �ӵ�

    private Animator animator;                                  // ĳ���� �ִϸ�����
    private CharacterController characterController;            // ĳ���� ��Ʈ�ѷ�
    private PlayerInputActions playerInputActions;              // �Է� �ý��� �׼�

    private Vector2 movement;                                   // �̵� �Է� ��
    private Vector3 lookDirection;                              // ���콺 ��ġ ��� ȸ�� ����
    private bool isRunning;                                     // �޸��� ����

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
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;
        playerInputActions.Enable();
    }

    // �Է� �׼� ��Ȱ��ȭ
    private void DisableInputActions()
    {
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Look.canceled -= OnLook;
        playerInputActions.Player.Run.performed -= OnRun;
        playerInputActions.Player.Run.canceled -= OnRun;
        playerInputActions.Disable();
    }

    // �̵� �Է� ó��
    private void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
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

    // �޸��� �Է� ó��
    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
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
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        float currentSpeed = runSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    // �ִϸ����� �Ķ���� ������Ʈ
    private void UpdateAnimatorParameters()
    {
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        float speedMultiplier = isRunning ? 2 : 1;

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
