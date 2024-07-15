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
    [Header("Human Set")]
    [SerializeField] float HumanrunSpeed = 5.0f;                   // 뛰기 속도
    [SerializeField] float _humanCurrentHp = 100;                      // 현재 체력 (값은 임의 설정)

    [Header("wolf Set")]
    [SerializeField] float maxRunSpeed = 9f;                      // 최대 뛰기 속도
    [SerializeField] float minSpeed = 4.5f;                       // 최소 이동 속도
    [SerializeField] float acceleration = 1.5f;                   // 가속도
    [SerializeField] float deceleration = 15f;                    // 감속도
    [SerializeField] private float currentSpeed = 0.0f;           // 현재 속도 (인스펙터에 노출)
    [SerializeField] float _wolfCurrentHp = 100;                      // 현재 체력 (값은 임의 설정)

    [Header("Comman Set")] private Animator animator;                                  // 캐릭터 애니메이터
    private CharacterController characterController;            // 캐릭터 컨트롤러
    private PlayerInputActions playerInputActions;              // 입력 시스템 액션
    private bool isMoving = false;                                // 이동 중 여부

    private Vector2 movement;                                   // 이동 입력 값
    private Vector3 lookDirection;                              // 마우스 위치 기반 회전 방향
    private string currentPlayerTag;
    private void Awake()
    {
        InitializeComponents();
        InitializeInputActions();
        InitializeSetPlayerTag();
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

    // 메인 플레이어인지 서브플레이어인지 찾아서 초기화
    private void InitializeSetPlayerTag()
    {
        currentPlayerTag = this.gameObject.CompareTag("Player") ? "Human" : "Wolf";

    }
    // 입력 액션 활성화
    private void EnableInputActions()
    {
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Look.performed += OnLook;
        playerInputActions.Player.Look.canceled += OnLook;
        playerInputActions.Enable();
    }

    // 입력 액션 비활성화
    private void DisableInputActions()
    {
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Look.canceled -= OnLook;
        playerInputActions.Disable();
    }

    // 이동 입력 처리
    private void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        if (currentPlayerTag == "Wolf")
        {
            isMoving = context.phase == InputActionPhase.Performed;
        }
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
        if (currentPlayerTag == "Human") // Human
        {
            Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
            float currentSpeed = HumanrunSpeed;
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        else // Wolf
        {
            if (isMoving)
            {
                // 가속도에 따라 속도 증가, 최소 속도 이상으로 설정
                currentSpeed += acceleration * Time.deltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxRunSpeed); // 최소 및 최대 속도 제한
            }
            else
            {
                // 감속도에 따라 속도 감소
                currentSpeed -= deceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0.0f); // 최소 속도 제한
            }

            Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

    }

    // 애니메이터 파라미터 업데이트
    private void UpdateAnimatorParameters()
    {
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y).normalized;
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        float speedMultiplier;

        if (currentPlayerTag == "Human")
        {
            speedMultiplier = 1;
        }
        else
        {
            speedMultiplier = currentSpeed / maxRunSpeed; // 현재 속도 비율
        }


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
