using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPlayerAttackManager : MonoBehaviour
{
    [Header("Hit Damage")]
    [SerializeField] private float hitDamage = 20f;

    [Header("Player variable")]
    [SerializeField] private Animator p_Anim;
    [SerializeField] public bool isAttacking = false; // 애니메이션 실행중 재입력 불가

    [Header("Hit Effect")]
    [SerializeField] private BoxCollider biteColi;
    [SerializeField] private int targetLayer;
    [SerializeField] private float hitEffectDalayTime; // 슬로우모션 지속 시간
    [SerializeField] private float slowMotionDuration; // 슬로우모션 지속 시간
    [SerializeField] private float slowMotionFactor; // 시간 느리게 가는 정도

    [Header("Hit Effect Timing")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitLoopTimeLimit = 2.0f;

    [Header("Hit Effect Spawn options")]
    [SerializeField] private float hitSpawnScale = 1.0f;

    [Header("Bite Effect Timing")]
    [SerializeField] private GameObject biteTrailEffect;

    [Header("Move Forward Timing")]
    [SerializeField] private float totalDistance; // 이동할 총 거리
    [SerializeField] private float moveForwardTime; // 이동 시간

    [Header("Ultimate Gauge Charge")]
    [SerializeField] private float _gaugeChargeValue;
    [SerializeField] private int maxHitChargeEnemyNum;
    [SerializeField] private int CounteHitEnemyNum;

    void Start()
    {
        // 타겟 레이어를 설정합니다. 예를 들어, 레이어 이름이 "Monster"라면:
        targetLayer = LayerMask.NameToLayer("Monster");

        // 임시적으로 검 콜라이더 끄기.
        biteColi.enabled = false;

        biteTrailEffect.SetActive(false);
    }

    // Update is called once per frame
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
    //    {
    //        OnAttack();
    //    }
    //}

    public void OnAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;  // 애니메이션 시작 시 공격 상태로 전환

            // 공격 애니메이션 트리거 실행
            p_Anim.SetTrigger("BasicAtk");

            biteColi.enabled = true;

            //StartCoroutine(MoveForwardByDiameter());
            CharacterRotation();
            StartCoroutine(SwordEffectLoop());
        }
    }

    private void CharacterRotation()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.y));

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }

    void OnTriggerEnter(Collider other)
    {
        //if (!other.TryGetComponent<SphereCollider>(out SphereCollider sphereCollider))
        //    return;

        SubPlayerSkillManager psm = transform.GetComponent<SubPlayerSkillManager>();
        // 충돌한 객체의 레이어가 타겟 레이어인지 확인합니다.
        //if (other.gameObject.layer == targetLayer)
        if (other.gameObject.layer == targetLayer && !psm.isCasting)
        {
            Debug.Log("Monster Collider와 충돌 감지!");

            CounteHitEnemyNum++;
            //Debug.Log($"Sub 맞은 Enemy 수 : {CounteHitEnemyNum}");

            // 여기서 충돌 처리를 합니다.
            MonsterView hitMonster = other.GetComponent<MonsterView>();
            if(hitMonster != null)
                hitMonster.HurtDamage(hitDamage, transform);

            //게이지 충전
            //DBCharacterPC.Instance.AddSkillGauge(_gaugeChargeValue);


            StartCoroutine(HitEffectLoop(other));
            StartCoroutine(SlowMotionEffect());
        }
    }

    IEnumerator SwordEffectLoop()
    {
        biteTrailEffect.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        biteColi.enabled = false;

        float elapsed = 0.0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + transform.forward * totalDistance;

        while (elapsed < moveForwardTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / moveForwardTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 마지막 위치 보정
        transform.position = targetPosition;

        yield return new WaitForSeconds(hitLoopTimeLimit);

        isAttacking = false;  // 공격 애니메이션이 끝난 후 공격 상태 해제

        biteTrailEffect.SetActive(false);
    }

    IEnumerator HitEffectLoop(Collider other)
    {
        yield return new WaitForSeconds(hitEffectDalayTime);
        
        //Vector3 hitPoint = other.ClosestPoint(transform.position); // 충돌 지점 추정
        Vector3 hitPoint = other.transform.position; // 충돌 지점 추정
        Vector3 newPosition = hitPoint + new Vector3(0, 0.1f, 0);

        GameObject effectPlayer = (GameObject)Instantiate(hitEffect, newPosition, transform.rotation);

        effectPlayer.transform.localScale = new Vector3(hitSpawnScale, hitSpawnScale, hitSpawnScale);

        //if (disableLights && effectPlayer.GetComponent<Light>())
        //{
        //    effectPlayer.GetComponent<Light>().enabled = false;
        //}
        //
        //if (disableSound && effectPlayer.GetComponent<AudioSource>())
        //{
        //    effectPlayer.GetComponent<AudioSource>().enabled = false;
        //}

        yield return new WaitForSeconds(hitLoopTimeLimit);

        Destroy(effectPlayer);
    }

    private IEnumerator SlowMotionEffect()
    {
        Time.timeScale = slowMotionFactor;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
    }

    // 여기에 transform rotation의 방향으로 transform position을 지름1만큼의 크기만큼 앞으로 변경하는 메서드 작성해줘
    IEnumerator MoveForwardByDiameter()
    {
        //float totalDistance = 6.0f; // 이동할 총 거리
        //float duration = 0.8f; // 이동하는 데 걸리는 시간
        float elapsed = 0.0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + transform.forward * totalDistance;

        while (elapsed < moveForwardTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / moveForwardTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 마지막 위치 보정
        transform.position = targetPosition;
    }

    public void SubStartAnim_CountingEnemyNum()
    {
        CounteHitEnemyNum = 0;
    }

    public void SubEndAnim_CalculatingGauge()
    {
        if (CounteHitEnemyNum >= maxHitChargeEnemyNum)
            CounteHitEnemyNum = maxHitChargeEnemyNum;

        //게이지 충전
        if (CounteHitEnemyNum > 0)
            DBCharacterPC.Instance.AddSkillGauge((float)CounteHitEnemyNum * _gaugeChargeValue);

        //Debug.Log($"Sub _gaugeChargeValue : {_gaugeChargeValue}");
        //Debug.Log($"Sub Gauge 추가량 : {(float)CounteHitEnemyNum * _gaugeChargeValue}");
    }
}
