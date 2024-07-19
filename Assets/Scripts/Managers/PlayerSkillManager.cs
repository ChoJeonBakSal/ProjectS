using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Skill Cool Time ���� ����")]
    [SerializeField] private Image Skill_Icon_Human_CollTimeBar;
    [SerializeField] private float playerSkillCoolTime;
    [SerializeField] private bool isPlayerSkillReady = true;

    [Header("Skill Damage")]
    [SerializeField] private float skillDamage = 60f;

    [Header("Player variable")]
    [SerializeField] private Animator p_Anim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] public bool isCasting = false; // �ִϸ��̼� ������ ���Է� �Ұ�

    [Header("Hit Effect")]
    [SerializeField] private SphereCollider skillCollider;
    [SerializeField] private Transform swordEndPoint;
    [SerializeField] private int targetLayer;
    [SerializeField] private float slowMotionDuration; // ���ο��� ���� �ð�
    [SerializeField] private float slowMotionFactor; // �ð� ������ ���� ����

    [Header("Hit Effect Timing")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitDelayTime = 1f;
    [SerializeField] private float hitLoopTimeLimit = 2.0f;

    [Header("Hit Effect Spawn options")]
    [SerializeField] private float hitSpawnScale = 1.0f;
    //public bool disableLights = true;
    //public bool disableSound = true;

    [Header("Skill Effect Timing")]
    [SerializeField] private GameObject swordEffect;
    [SerializeField] private GameObject swordTrailEffect;
    [SerializeField] private float beforeSkillEffectDelayTimeScale = 1.0f;
    //private Quaternion additionalRotation = Quaternion.Euler(90, 0, -90);
    [SerializeField] private float swordLoopTimeLimit = 2.0f;
    [SerializeField] private float skillEffectDelayTime = 1.0f;

    [Header("Skill Effect Spawn options")]
    private bool disableLights = true;
    private bool disableSound = true;
    [SerializeField] private float swordSpawnScale = 1.0f;

    [Header("Ultimate Gauge Charge")]
    [SerializeField] private float _gaugeChargeValue;
    [SerializeField] private int maxHitChargeEnemyNum;
    [SerializeField] private int CounteHitEnemyNum;

    void Start()
    {
        // Ÿ�� ���̾ �����մϴ�. ���� ���, ���̾� �̸��� "Monster"���:
        targetLayer = LayerMask.NameToLayer("Monster");

        // �ӽ������� �� �ݶ��̴� ����.
        skillCollider.enabled = false;

        swordTrailEffect.SetActive(false);

        rb = GetComponent<Rigidbody>();
    }

    public void OnSkill()
    {
        if (!isCasting && isPlayerSkillReady)
        {
            StartCoroutine(PlayerSkillCooldown());

            swordTrailEffect.SetActive(true);

            isCasting = true;  // �ִϸ��̼� ���� �� ���� ���·� ��ȯ

            //skillCollider.enabled = false;

            // ���� �ִϸ��̼� Ʈ���� ����
            p_Anim.SetTrigger("BasicSkill");
            Time.timeScale = beforeSkillEffectDelayTimeScale;
        }
    }
    IEnumerator PlayerSkillCooldown()
    {
        isPlayerSkillReady = false;
        Skill_Icon_Human_CollTimeBar.fillAmount = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < playerSkillCoolTime)
        {
            elapsedTime += Time.deltaTime;
            Skill_Icon_Human_CollTimeBar.fillAmount = elapsedTime / playerSkillCoolTime;
            yield return null;
        }

        Skill_Icon_Human_CollTimeBar.fillAmount = 1f;
        isPlayerSkillReady = true;
        Debug.Log("�÷��̾� ��ų�� �ٽ� �غ�Ǿ����ϴ�.");
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerAttackManager pam = transform.GetComponent<PlayerAttackManager>();
        // �浹�� ��ü�� ���̾ Ÿ�� ���̾����� Ȯ���մϴ�.
        if (other.gameObject.layer == targetLayer && !pam.isAttacking)
        {
            Debug.Log("Monster Collider�� �浹 ����!");

            CounteHitEnemyNum++;
            Debug.Log($"���� Enemy �� : {CounteHitEnemyNum}");

            // ���⼭ �浹 ó���� �մϴ�.
            MonsterView hitMonster = other.GetComponent<MonsterView>();
            hitMonster.HurtDamage(skillDamage, transform);

            StartCoroutine(HitEffectLoop(other));
            StartCoroutine(SlowMotionEffect());
        }
    }

    private IEnumerator SlowMotionEffect()
    {
        Time.timeScale = slowMotionFactor;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
    }


    IEnumerator SkillEffectLoop()
    {
        //Quaternion combinedRotation = transform.rotation * additionalRotation;
        Vector3 newPosition = swordEndPoint.position; // Transform�� ��ġ�� ���
        //Vector3 newPosition = new Vector3(swordEndPoint.x, 0.1f, swordEndPoint.z);

        yield return new WaitForSeconds(skillEffectDelayTime);

        //GameObject effectPlayer = (GameObject)Instantiate(swordEffect, newPosition, transform.rotation);
        GameObject effectPlayer = (GameObject)Instantiate(swordEffect, newPosition, transform.rotation);

        effectPlayer.transform.localScale = new Vector3(swordSpawnScale, swordSpawnScale, swordSpawnScale);

        if (disableLights && effectPlayer.GetComponent<Light>())
        {
            effectPlayer.GetComponent<Light>().enabled = false;
        }

        if (disableSound && effectPlayer.GetComponent<AudioSource>())
        {
            effectPlayer.GetComponent<AudioSource>().enabled = false;
        }

        yield return new WaitForSeconds(swordLoopTimeLimit);

        Destroy(effectPlayer);

        isCasting = false;  // ���� �ִϸ��̼��� ���� �� ���� ���� ����

        swordTrailEffect.SetActive(false);
    }

    IEnumerator HitEffectLoop(Collider other)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position); // �浹 ���� ����
        Vector3 newPosition = hitPoint + new Vector3(0, 1, 0);

        yield return new WaitForSeconds(hitDelayTime);

        GameObject effectPlayer = (GameObject)Instantiate(hitEffect, newPosition, transform.rotation);

        effectPlayer.transform.localScale = new Vector3(hitSpawnScale, hitSpawnScale, hitSpawnScale);

        if (disableLights && effectPlayer.GetComponent<Light>())
        {
            effectPlayer.GetComponent<Light>().enabled = false;
        }

        if (disableSound && effectPlayer.GetComponent<AudioSource>())
        {
            effectPlayer.GetComponent<AudioSource>().enabled = false;
        }

        yield return new WaitForSeconds(hitLoopTimeLimit);

        Destroy(effectPlayer);
    }

    // ��ų �ִϸ��̼ǿ��� ����
    void TriggerOn()
    {
        StartCoroutine(OffTrigger());
    }

    IEnumerator OffTrigger()
    {
        skillCollider.enabled = true;
        StartCoroutine(SkillEffectLoop());
        yield return new WaitForSeconds(0.1f);
        skillCollider.enabled = false;
    }

    public void PlayerSkillStartAnim_CountingEnemyNum()
    {
        CounteHitEnemyNum = 0;
    }

    public void PlayerSkillEndAnim_CalculatingGauge()
    {
        if (CounteHitEnemyNum >= maxHitChargeEnemyNum)
            CounteHitEnemyNum = maxHitChargeEnemyNum;

        //������ ����
        if (CounteHitEnemyNum > 0)
            DBCharacterPC.Instance.AddSkillGauge((float)CounteHitEnemyNum * _gaugeChargeValue);

        Debug.Log($"Gauge �߰��� : {(float)CounteHitEnemyNum * _gaugeChargeValue}");
    }
}
