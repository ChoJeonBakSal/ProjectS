using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Human Set")] // 휴먼                              [ 모든 값은 임의 설정 ]
    [SerializeField] float HumanrunSpeed = 5.0f;                   // 뛰기 속도

    [Header("Wolf Set")]  // 울프
    [SerializeField] float maxRunSpeed = 9f;                       // 최대 뛰기 속도
    [SerializeField] float minSpeed = 4.5f;                        // 최소 이동 속도
    [SerializeField] float acceleration = 1.5f;                    // 가속도
    [SerializeField] float deceleration = 15f;                     // 감속도
    [SerializeField] private float currentSpeed = 0.0f;            // 현재 속도 (인스펙터에 노출)

    [Header("Common Set")] // 공통

    private Animator animator;                                     // 캐릭터 애니메이터    
    private NavMeshAgent navMeshAgent;                             // 네브메쉬 에이전트
    private PlayerInputActions playerInputActions;                 // 입력 시스템 액션
    private bool isMoving = false;                                 // 이동 중 여부
    private Vector2 movement;                                      // 이동 입력 값
    private Vector3 lookDirection;                                 // 마우스 위치 기반 회전 방향
    private string currentPlayerTag;
    public float HumanInitHp = 100f;                               // 휴먼 초기 체력
    public float WolfInitHp = 150f;                                // 울프 초기 체력
    [SerializeField] private float _currentHp;
    public bool IsCurrentPlayerHuman { get; set; }                 // 현재 조작 중인 플레이어가 휴먼인지 여부
    public PlayerController target;
    public float followDistance = 3.0f;                            // 타겟과 유지할 거리

    public float InitHp { get; private set; }
    public float CurrentHp
    {
        get { return _currentHp; }
        private set
        {
            _currentHp = Mathf.Max(0, value); // 0 이하로 떨어지지 않도록 설정
            if (_currentHp == 0)
            {
                Debug.Log($"{currentPlayerTag} character is dead.");
            }
        }
    }

    private void Awake()
    {
        InitializeComponents();
        InitializeInputActions();
        InitializeSetPlayerTag();
        InitializeSetPlayerHp();
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
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false; // 네브메쉬 에이전트가 자동으로 회전하지 않도록 설정
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
    private void InitializeSetPlayerHp()
    {
        if (currentPlayerTag == "Human")
        {
            InitHp = HumanInitHp;
        }
        else if (currentPlayerTag == "Wolf")
        {
            InitHp = WolfInitHp;
        }
        CurrentHp = InitHp;
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
        if (currentPlayerTag != null && currentPlayerTag == "Wolf")
        {
            isMoving = context.phase == InputActionPhase.Performed;
        }
    }

    // 마우스 위치 입력 처리
    private void OnLook(InputAction.CallbackContext context)
    {
        if (IsCurrentPlayerHuman)
        {
            Vector3 mousePos = context.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                lookDirection = hitInfo.point - transform.position;
                lookDirection.y = 0; // Y축 회전 방지
            }
        }

    }

    // 매 프레임마다 이동 및 애니메이터 업데이트
    private void Update()
    {
        if (!IsCurrentPlayerHuman)
        {
            UpdateAnimatorParameters();
            FollowTarget();
        }
        else
        {
            MoveCharacter();
            UpdateAnimatorParameters();
        }
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

        if (currentPlayerTag == "Human") // Human
        {
            float currentSpeed = HumanrunSpeed;
            navMeshAgent.speed = currentSpeed;
            navMeshAgent.Move(moveDirection * currentSpeed * Time.deltaTime);
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

            navMeshAgent.speed = currentSpeed;
            navMeshAgent.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
    }

    // 애니메이터 파라미터 업데이트
    private void UpdateAnimatorParameters()
    {
        if (IsCurrentPlayerHuman)
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

    private void HitDamage(float damage) // 때리는 곳에서 호출하면 됨.
    {
        CurrentHp -= damage;
    }
    // 타겟 추적 및 거리 유지
    private void FollowTarget()
    {
        Debug.Log(currentPlayerTag + " :: " + IsCurrentPlayerHuman);

        if (target != null && !IsCurrentPlayerHuman)
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 wolfPosition = transform.position;
            float distance = Vector3.Distance(targetPosition, wolfPosition);

            // 플레이어의 이동 방향 벡터 계산
            Vector3 moveDirection = (targetPosition - wolfPosition).normalized;
            Vector3 rightOffset = target.transform.right * followDistance * 3f; // 옆으로 더 작은 오프셋 적용
            Vector3 forwardOffset = moveDirection * followDistance * 3f; // 플레이어 앞에 오프셋 적용

            // 동반자를 플레이어의 왼쪽이나 오른쪽에 위치시키기 위한 오프셋 계산
            Vector3 desiredPosition = targetPosition + rightOffset + forwardOffset; // 기본적으로 오른쪽으로 설정하고 앞쪽으로 약간 이동
            if (Vector3.Dot(target.transform.right, wolfPosition - targetPosition) < 0)
            {
                desiredPosition = targetPosition - rightOffset + forwardOffset; // 플레이어의 왼쪽에 위치하고 앞쪽으로 약간 이동
            }

            // 플레이어와 동반자 간의 거리가 일정 거리 이하로 줄어들지 않도록 설정
            if (distance > followDistance)
            {
                navMeshAgent.SetDestination(desiredPosition);

                // 속도 설정
                isMoving = true;
                currentSpeed += acceleration * Time.deltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxRunSpeed); // 최소 및 최대 속도 제한

                navMeshAgent.speed = currentSpeed;

                // 애니메이터 파라미터 업데이트
                UpdateFollowerAnimatorParameters();
            }
            else
            {
                navMeshAgent.ResetPath(); // 타겟과 일정 거리를 유지하면 멈춤
                isMoving = false;

                // 속도 설정
                currentSpeed -= deceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0.0f); // 최소 속도 제한

                navMeshAgent.speed = currentSpeed;

                // 애니메이터 파라미터 업데이트
                UpdateFollowerAnimatorParameters();
            }

            // 동반자의 회전을 네브메시 에이전트의 이동 방향에 맞추기
            if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
            {
                Quaternion targetRotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    private void UpdateFollowerAnimatorParameters()
    {
        // 현재 속도 비율
        float speedMultiplier = currentSpeed / maxRunSpeed;

        // 모든 방향에 대해 앞으로 가는 애니메이션만 재생하도록 설정
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", speedMultiplier);
    }



}
