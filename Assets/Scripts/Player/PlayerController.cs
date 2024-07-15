using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Human Set")] // �޸�                              [ ��� ���� ���� ���� ]
    [SerializeField] float HumanrunSpeed = 5.0f;                   // �ٱ� �ӵ�

    [Header("Wolf Set")]  // ����
    [SerializeField] float maxRunSpeed = 9f;                       // �ִ� �ٱ� �ӵ�
    [SerializeField] float minSpeed = 4.5f;                        // �ּ� �̵� �ӵ�
    [SerializeField] float acceleration = 1.5f;                    // ���ӵ�
    [SerializeField] float deceleration = 15f;                     // ���ӵ�
    [SerializeField] private float currentSpeed = 0.0f;            // ���� �ӵ� (�ν����Ϳ� ����)

    [Header("Common Set")] // ����

    private Animator animator;                                     // ĳ���� �ִϸ�����    
    private NavMeshAgent navMeshAgent;                             // �׺�޽� ������Ʈ
    private PlayerInputActions playerInputActions;                 // �Է� �ý��� �׼�
    private bool isMoving = false;                                 // �̵� �� ����
    private Vector2 movement;                                      // �̵� �Է� ��
    private Vector3 lookDirection;                                 // ���콺 ��ġ ��� ȸ�� ����
    private string currentPlayerTag;
    public float HumanInitHp = 100f;                               // �޸� �ʱ� ü��
    public float WolfInitHp = 150f;                                // ���� �ʱ� ü��
    [SerializeField]private float _currentHp;
    public bool IsCurrentPlayerHuman { get; set; }                 // ���� ���� ���� �÷��̾ �޸����� ����
    public GameObject targetObj;
    public float followDistance = 2.0f;                            // Ÿ�ٰ� ������ �Ÿ�

    public float InitHp { get; private set; }
    public float CurrentHp
    {
        get { return _currentHp; }
        private set
        {
            _currentHp = Mathf.Max(0, value); // 0 ���Ϸ� �������� �ʵ��� ����
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

    // ������Ʈ �ʱ�ȭ
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false; // �׺�޽� ������Ʈ�� �ڵ����� ȸ������ �ʵ��� ����
    }

    // �Է� �׼� �ʱ�ȭ
    private void InitializeInputActions()
    {
        playerInputActions = new PlayerInputActions();
    }

    // ���� �÷��̾����� �����÷��̾����� ã�Ƽ� �ʱ�ȭ
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
        if (currentPlayerTag != null && currentPlayerTag == "Wolf")
        {
            isMoving = context.phase == InputActionPhase.Performed;
        } 
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
        FollowTarget();
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

            navMeshAgent.speed = currentSpeed;
            navMeshAgent.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
    }

    // �ִϸ����� �Ķ���� ������Ʈ
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
            speedMultiplier = currentSpeed / maxRunSpeed; // ���� �ӵ� ����
        }

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

    private void HitDamage(float damage) // ������ ������ ȣ���ϸ� ��.
    {
        CurrentHp -= damage;
    }
    // Ÿ�� ���� �� �Ÿ� ����
    private void FollowTarget()
    {
        Debug.Log(currentPlayerTag + " :: " + IsCurrentPlayerHuman);

        if (targetObj != null && !IsCurrentPlayerHuman)
        {
            float distance = Vector3.Distance(targetObj.transform.position, transform.position);
            if (distance > followDistance)
            {
                navMeshAgent.SetDestination(targetObj.transform.position);
            }
            else
            {
                navMeshAgent.ResetPath(); // Ÿ�ٰ� ���� �Ÿ��� �����ϸ� ����
            }
        }
    }
}
