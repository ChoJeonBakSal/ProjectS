using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Skill Cool Time 관련 변수")]
    [SerializeField] private Image Skill_Icon_Human_CollTimeBar;
    [SerializeField] private float playerSkillCoolTime;
    [SerializeField] private bool isPlayerSkillReady = true;

    [Header("Skill Damage")]
    [SerializeField] private float skillDamage = 60f;

    [Header("Player variable")]
    [SerializeField] private Animator p_Anim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] public bool isCasting = false; // 애니메이션 실행중 재입력 불가

    [Header("Hit Effect")]
    [SerializeField] private SphereCollider skillCollider;
    [SerializeField] private Transform swordEndPoint;
    [SerializeField] private int targetLayer;
    [SerializeField] private float slowMotionDuration; // 슬로우모션 지속 시간
    [SerializeField] private float slowMotionFactor; // 시간 느리게 가는 정도

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
        // 타겟 레이어를 설정합니다. 예를 들어, 레이어 이름이 "Monster"라면:
        targetLayer = LayerMask.NameToLayer("Monster");

        // 임시적으로 검 콜라이더 끄기.
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

            isCasting = true;  // 애니메이션 시작 시 공격 상태로 전환

            //skillCollider.enabled = false;

            // 공격 애니메이션 트리거 실행
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
        Debug.Log("플레이어 스킬이 다시 준비되었습니다.");
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerAttackManager pam = transform.GetComponent<PlayerAttackManager>();
        // 충돌한 객체의 레이어가 타겟 레이어인지 확인합니다.
        if (other.gameObject.layer == targetLayer && !pam.isAttacking)
        {
            Debug.Log("Monster Collider와 충돌 감지!");

            CounteHitEnemyNum++;
            Debug.Log($"맞은 Enemy 수 : {CounteHitEnemyNum}");

            // 여기서 충돌 처리를 합니다.
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
        Vector3 newPosition = swordEndPoint.position; // Transform의 위치를 사용
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

        isCasting = false;  // 공격 애니메이션이 끝난 후 공격 상태 해제

        swordTrailEffect.SetActive(false);
    }

    IEnumerator HitEffectLoop(Collider other)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position); // 충돌 지점 추정
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

    // 스킬 애니메이션에서 참조
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

        //게이지 충전
        if (CounteHitEnemyNum > 0)
            DBCharacterPC.Instance.AddSkillGauge((float)CounteHitEnemyNum * _gaugeChargeValue);

        Debug.Log($"Gauge 추가량 : {(float)CounteHitEnemyNum * _gaugeChargeValue}");
    }
}
