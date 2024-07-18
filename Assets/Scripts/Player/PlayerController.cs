using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("MainUI Ultimate Skill Gauge")]
    [SerializeField] private GameObject MainUI;

    [Header("Layer�� ��ų ����")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject subPlayerCamera;
    [SerializeField] private GameObject UtimateSkillManagerObj;
    [SerializeField] private UtimateSkillManager UtimateSkillManager;

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
    public string currentPlayerTag { get; private set; }
    public float HumanInitHp = 100f;                               // �޸� �ʱ� ü��
    public float WolfInitHp = 150f;                                // ���� �ʱ� ü��
    [SerializeField] private float _currentHp;
    public bool IsCurrentPlayerHuman { get; set; }                 // ���� ���� ���� �÷��̾��ΰ��� Ȯ��
    public PlayerController target;
    public float followDistance = 3.0f;                            // Ÿ�ٰ� ������ �Ÿ�

    public bool IsAttacking;

    public int playerDataInfoID {  get; private set; }
    public float InitATk {  get; private set; }
    public int NormalAttackID {  get; private set; }
    public int SkillAttackID {  get; private set; }

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

        UtimateSkillManagerObj = GameObject.Find("UtimateSkillManager");
        UtimateSkillManager = UtimateSkillManagerObj.GetComponent<UtimateSkillManager>();
    }

    private void OnEnable()
    {
        EnableInputActions();

        IsAttacking = false;
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
        playerDataInfoID = DBCharacterPC.Instance.GetInfoDBID(currentPlayerTag);
        InitATk = DBCharacterPC.Instance.GetAttackDamageValue(playerDataInfoID);
        NormalAttackID = DBCharacterPC.Instance.GetNormalAttackID(playerDataInfoID);
        SkillAttackID = DBCharacterPC.Instance.GetSkillAttackID(playerDataInfoID);
    }
    // �Է� �׼� Ȱ��ȭ
    private void EnableInputActions()
    {
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;

        // �ΰ� ��Ÿ
        playerInputActions.Player.BasicAtk.performed += OnBasicAtk;
        playerInputActions.Player.BasicAtk.canceled += OnBasicAtk;

        // �ΰ� ��ų
        playerInputActions.Player.BasicSkill.performed += OnBasicSkill;
        playerInputActions.Player.BasicSkill.canceled += OnBasicSkill;

        // �ñر�
        playerInputActions.Player.UltimateSkill.performed += OnUltimateSkill;
        playerInputActions.Player.UltimateSkill.canceled += OnUltimateSkill;

        //playerInputActions.Player.Look.performed += OnLook;
        //playerInputActions.Player.Look.canceled += OnLook;
        playerInputActions.Enable();
    }

    // �Է� �׼� ��Ȱ��ȭ
    private void DisableInputActions()
    {
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;

        // �ΰ� ��Ÿ
        playerInputActions.Player.BasicAtk.performed -= OnBasicAtk;
        playerInputActions.Player.BasicAtk.canceled -= OnBasicAtk;

        // �ΰ� ��ų
        playerInputActions.Player.BasicSkill.performed -= OnBasicSkill;
        playerInputActions.Player.BasicSkill.canceled -= OnBasicSkill;

        // �ñر�
        playerInputActions.Player.UltimateSkill.performed -= OnUltimateSkill;
        playerInputActions.Player.UltimateSkill.canceled -= OnUltimateSkill;

        //playerInputActions.Player.Look.performed -= OnLook;
        //playerInputActions.Player.Look.canceled -= OnLook;
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

    private void OnBasicAtk(InputAction.CallbackContext context)
    {
        // playerCamera�� true       subPlayerCamera�� false �϶� �ΰ� ��Ŀ��
        if (playerCamera.activeSelf && !subPlayerCamera.activeSelf)
        {
            Debug.Log("�ΰ� ����");
            PlayerAttackManager pam = GameObject.Find("Human").GetComponent<PlayerAttackManager>();
            PlayerSkillManager psm = GameObject.Find("Human").GetComponent<PlayerSkillManager>();
            if (!pam.isAttacking && !psm.isCasting)
                pam.OnAttack();
        }
        else if (!playerCamera.activeSelf && subPlayerCamera.activeSelf)
        {
            Debug.Log("���� ����");
            SubPlayerAttackManager sam = GameObject.Find("Wolf").GetComponent<SubPlayerAttackManager>();
            SubPlayerSkillManager ssm = GameObject.Find("Wolf").GetComponent<SubPlayerSkillManager>();
            if (!sam.isAttacking && !ssm.isCasting)
                sam.OnAttack();
        }

    }

    private void OnBasicSkill(InputAction.CallbackContext context)
    {
        if (playerCamera.activeSelf && !subPlayerCamera.activeSelf)
        {
            Debug.Log("�ΰ� ��ų ����");
            PlayerAttackManager pam = GameObject.Find("Human").GetComponent<PlayerAttackManager>();
            PlayerSkillManager psm = GameObject.Find("Human").GetComponent<PlayerSkillManager>();
            if(!pam.isAttacking && !psm.isCasting)
                psm.OnSkill();
        }
        else if (!playerCamera.activeSelf && subPlayerCamera.activeSelf)
        {
            Debug.Log("���� ��ų ����");
            SubPlayerAttackManager sam = GameObject.Find("Wolf").GetComponent<SubPlayerAttackManager>();
            SubPlayerSkillManager ssm = GameObject.Find("Wolf").GetComponent<SubPlayerSkillManager>();
            if (!sam.isAttacking && !ssm.isCasting)
                ssm.OnCasting();
        }

    }

    void OnUltimateSkill(InputAction.CallbackContext context)
    {
        MainUI mui = this.MainUI.GetComponent<MainUI>();
        if (mui._crtSkillGauge != 100)
            return;
        else
            mui._crtSkillGauge = 0;


        Debug.Log("�ñر� ����");

        if (playerCamera.activeSelf && !subPlayerCamera.activeSelf)
        {
            PlayerAttackManager pam = GameObject.Find("Human").GetComponent<PlayerAttackManager>();
            PlayerSkillManager psm = GameObject.Find("Human").GetComponent<PlayerSkillManager>();
            if (!pam.isAttacking && !psm.isCasting)
                UtimateSkillManager.OnSkill();
        }
        else if (!playerCamera.activeSelf && subPlayerCamera.activeSelf)
        {
            SubPlayerAttackManager sam = GameObject.Find("Wolf").GetComponent<SubPlayerAttackManager>();
            SubPlayerSkillManager ssm = GameObject.Find("Wolf").GetComponent<SubPlayerSkillManager>();
            if (!sam.isAttacking && !ssm.isCasting)
                UtimateSkillManager.OnSkill();
        }
    }

    // ���콺 ��ġ �Է� ó��
    //private void OnLook(InputAction.CallbackContext context)
    //{
    //    if (IsAttacking) return;
    //    if (IsCurrentPlayerHuman)
    //    {
    //        Vector3 mousePos = context.ReadValue<Vector2>();
    //        Ray ray = Camera.main.ScreenPointToRay(mousePos);
    //        if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, (1 << LayerMask.NameToLayer("Ground"))))
    //        {
    //            lookDirection = hitInfo.point - transform.position;
    //            lookDirection.y = 0; // Y�� ȸ�� ����
    //        }
    //    }

    //}

    // �� �����Ӹ��� �̵� �� �ִϸ����� ������Ʈ
    private void Update()
    {
        if (!IsCurrentPlayerHuman)
        {
            if(currentPlayerTag == "Human")
            {
                FollowHumanTarget();

            }
            else
            {
                FollowTarget();
            }            
        }
        else
        {
            CharacterRotation();
            MoveCharacter();
        }
            UpdateAnimatorParameters();
    }

    private void CharacterRotation()
    {
        if (IsAttacking) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.y));

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }

    // �� ������ �Ĺݿ� ĳ���� ȸ�� ó��
    private void LateUpdate()
    {
        RotateToMouse();
    }

    // ĳ���� �̵� ó��
    private void MoveCharacter()
    {
        if (IsAttacking) return;

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
                speedMultiplier = currentSpeed / maxRunSpeed; // ���� �ӵ� ����
            }

            animator.SetFloat("Horizontal", localMove.x * speedMultiplier);
            animator.SetFloat("Vertical", localMove.z * speedMultiplier);
        }

    }

    // ĳ���͸� ���콺 �������� ȸ��
    private void RotateToMouse()
    {        
        if (!IsCurrentPlayerHuman) return;                 
      
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

    #region Wolf
    private void FollowTarget()
    {
        //Debug.Log(currentPlayerTag + " :: " + IsCurrentPlayerHuman);

        if (target != null && currentPlayerTag == "Wolf")
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 wolfPosition = transform.position;
            float distance = Vector3.Distance(targetPosition, wolfPosition);

            // �÷��̾��� �̵� ���� ���� ���
            Vector3 moveDirection = (targetPosition - wolfPosition).normalized;
            Vector3 rightOffset = target.transform.right * followDistance * 3f; // ������ �� ���� ������ ����
            Vector3 forwardOffset = moveDirection * followDistance * 3f; // �÷��̾� �տ� ������ ����

            // �����ڸ� �÷��̾��� �����̳� �����ʿ� ��ġ��Ű�� ���� ������ ���
            Vector3 desiredPosition = targetPosition + rightOffset + forwardOffset; // �⺻������ ���������� �����ϰ� �������� �ణ �̵�
            if (Vector3.Dot(target.transform.right, wolfPosition - targetPosition) < 0)
            {
                desiredPosition = targetPosition - rightOffset + forwardOffset; // �÷��̾��� ���ʿ� ��ġ�ϰ� �������� �ణ �̵�
            }

            // �÷��̾�� ������ ���� �Ÿ��� ���� �Ÿ� ���Ϸ� �پ���� �ʵ��� ����
            if (distance > followDistance)
            {
                navMeshAgent.SetDestination(desiredPosition);

                // �ӵ� ����
                isMoving = true;
                currentSpeed += acceleration * Time.deltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxRunSpeed); // �ּ� �� �ִ� �ӵ� ����

                navMeshAgent.speed = currentSpeed;

                // �ִϸ����� �Ķ���� ������Ʈ
                UpdateFollowerAnimatorParameters();
            }
            else
            {
                navMeshAgent.ResetPath(); // Ÿ�ٰ� ���� �Ÿ��� �����ϸ� ����
                isMoving = false;

                // �ӵ� ����
                currentSpeed -= deceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0.0f); // �ּ� �ӵ� ����

                navMeshAgent.speed = currentSpeed;

                // �ִϸ����� �Ķ���� ������Ʈ
                UpdateFollowerAnimatorParameters();
            }

            // �������� ȸ���� �׺�޽� ������Ʈ�� �̵� ���⿡ ���߱�
            if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
            {
                Debug.Log("ȸ����...");
                Quaternion targetRotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(target.transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    private void UpdateFollowerAnimatorParameters()
    {
        // ���� �ӵ� ����
        float speedMultiplier = currentSpeed / maxRunSpeed;

        // ��� ���⿡ ���� ������ ���� �ִϸ��̼Ǹ� ����ϵ��� ����
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", speedMultiplier);
    }
    #endregion

    #region Human
    private void FollowHumanTarget()
    {
        if (target != null && currentPlayerTag == "Human")
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 humanPosition = transform.position;
            float distance = Vector3.Distance(targetPosition, humanPosition);

            // �÷��̾��� �̵� ���� ���� ���
            Vector3 moveDirection = (targetPosition - humanPosition).normalized;
            Vector3 rightOffset = target.transform.right * followDistance * 3f; // ������ �� ū ������ ����
            Vector3 forwardOffset = moveDirection * followDistance * 3f; // �÷��̾� �տ� ������ ����

            // �����ڸ� �÷��̾��� �����̳� �����ʿ� ��ġ��Ű�� ���� ������ ���
            Vector3 desiredPosition = targetPosition + rightOffset + forwardOffset; // �⺻������ ���������� �����ϰ� �������� �ణ �̵�
            if (Vector3.Dot(target.transform.right, humanPosition - targetPosition) < 0)
            {
                desiredPosition = targetPosition - rightOffset + forwardOffset; // �÷��̾��� ���ʿ� ��ġ�ϰ� �������� �ణ �̵�
            }

            // �÷��̾�� ������ ���� �Ÿ��� ���� �Ÿ� ���Ϸ� �پ���� �ʵ��� ����
            if (distance > followDistance)
            {
                navMeshAgent.SetDestination(desiredPosition);

                // �ӵ� ����
                navMeshAgent.speed = HumanrunSpeed;

                // �ִϸ����� �Ķ���� ������Ʈ
                UpdateHumanAnimatorParameters(moveDirection);
            }
            else
            {
                navMeshAgent.ResetPath(); // Ÿ�ٰ� ���� �Ÿ��� �����ϸ� ����

                // �ִϸ����� �Ķ���� ������Ʈ
                UpdateHumanAnimatorParameters(Vector3.zero);
            }

            // �������� ȸ���� �׺�޽� ������Ʈ�� �̵� ���⿡ ���߱�
            if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
            {
                Quaternion targetRotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
    private void UpdateHumanAnimatorParameters(Vector3 moveDirection)
    {
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);
        animator.SetFloat("Horizontal", localMove.x);
        animator.SetFloat("Vertical", localMove.z);
    }

    #endregion

}
