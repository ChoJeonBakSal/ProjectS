using UnityEngine;
using UnityEngine.InputSystem;

/* [ Memo ]
  //0712
  - 플레이어가 바라보는 방향은 마우스 포인터로 설정
  - 8방향 애니메이션 적용 기본, 걷기, 뛰기
  - 걷기 ASDW 뛰기 Shift
  - Blend Tree 사용
  - 조작 시 카메라 필요
 */

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float runSpeed = 5.0f;                     // 뛰기 속도

    private Animator animator;                                  // 캐릭터 애니메이터
    private CharacterController characterController;            // 캐릭터 컨트롤러
    private PlayerInputActions playerInputActions;              // 입력 시스템 액션

    private Vector2 movement;                                   // 이동 입력 값
    private Vector3 lookDirection;                              // 마우스 위치 기반 회전 방향
    private bool isRunning;                                     // 달리기 상태

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

    // 컴포넌트 초기화
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    // 입력 액션 초기화
    private void InitializeInputActions()
    {
        playerInputActions = new PlayerInputActions();
    }

    // 입력 액션 활성화
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

    // 입력 액션 비활성화
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

    // 이동 입력 처리
    private void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    // 마우스 위치 입력 처리
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

    // 달리기 입력 처리
    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    // 매 프레임마다 이동 및 애니메이터 업데이트
    private void Update()
    {
        MoveCharacter();
        UpdateAnimatorParameters();
    }

    // 매 프레임 후반에 캐릭터 회전 처리
    private void LateUpdate()
    {
        RotateToMouse();
    }

    // 캐릭터 이동 처리
    private void MoveCharacter()
    {
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        float currentSpeed = runSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    // 애니메이터 파라미터 업데이트
    private void UpdateAnimatorParameters()
    {
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        float speedMultiplier = isRunning ? 2 : 1;

        animator.SetFloat("Horizontal", localMove.x * speedMultiplier);
        animator.SetFloat("Vertical", localMove.z * speedMultiplier);
    }

    // 캐릭터를 마우스 방향으로 회전
    private void RotateToMouse()
    {
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
