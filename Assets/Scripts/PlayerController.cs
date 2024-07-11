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

        // PlayerInputActions 초기화
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // 액션에 콜백 등록
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Look.performed += OnLook;
        playerInputActions.Player.Look.canceled += OnLook;
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;

        // PlayerInputActions 활성화
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        // 액션에서 콜백 제거
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Look.canceled -= OnLook;
        playerInputActions.Player.Run.performed -= OnRun;
        playerInputActions.Player.Run.canceled -= OnRun;

        // PlayerInputActions 비활성화
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
            lookDirection.y = 0; // Y축 회전 방지
        }
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private void Update()
    {
        // 캐릭터 이동 처리
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // 애니메이터 매개변수 업데이트
        float horizontal = Vector3.Dot(moveDirection, transform.right);
        float vertical = Vector3.Dot(moveDirection, transform.forward);

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", moveDirection.magnitude); // 이동 속도에 따라 설정

        // 캐릭터를 마우스 방향으로 회전
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
