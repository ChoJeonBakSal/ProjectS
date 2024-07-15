using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Player variable")]
    [SerializeField] private Animator p_Anim;
    [SerializeField] private bool isCasting = false; // 애니메이션 실행중 재입력 불가


    [Header("Hit Effect")]
    [SerializeField] private SphereCollider skillCollider;
    [SerializeField] private int targetLayer;
    [SerializeField] private float slowMotionDuration; // 슬로우모션 지속 시간
    [SerializeField] private float slowMotionFactor; // 시간 느리게 가는 정도

    [Header("Hit Effect Timing")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitLoopTimeLimit = 2.0f;

    [Header("Hit Effect Spawn options")]
    [SerializeField] private float hitSpawnScale = 1.0f;
    //public bool disableLights = true;
    //public bool disableSound = true;

    [Header("Sword Effect Timing")]
    [SerializeField] private GameObject swordEffect;
    private Quaternion additionalRotation = Quaternion.Euler(90, 0, -90);
    [SerializeField] private float swordLoopTimeLimit = 2.0f;
    [SerializeField] private float swordEffectDelayTime = 1.0f;

    [Header("Sword Effect Spawn options")]
    private bool disableLights = true;
    private bool disableSound = true;
    [SerializeField] private float swordSpawnScale = 1.0f;


    void Start()
    {
        // 타겟 레이어를 설정합니다. 예를 들어, 레이어 이름이 "Monster"라면:
        targetLayer = LayerMask.NameToLayer("Monster");

        // 임시적으로 검 콜라이더 끄기.
        skillCollider.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isCasting)
        {
            OnSkill();
        }
    }

    void OnSkill()
    {
        isCasting = true;  // 애니메이션 시작 시 공격 상태로 전환

        //skillCollider.enabled = false;

        // 공격 애니메이션 트리거 실행
        p_Anim.SetTrigger("BasicSkill");

        StartCoroutine(SwordEffectLoop());
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌한 객체의 레이어가 타겟 레이어인지 확인합니다.
        if (other.gameObject.layer == targetLayer)
        {
            Debug.Log("Monster Collider와 충돌 감지!");

            // 여기서 충돌 처리를 합니다.
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


    IEnumerator SwordEffectLoop()
    {
        Quaternion combinedRotation = transform.rotation * additionalRotation;
        Vector3 newPosition = transform.position + new Vector3(0, 1, 0);

        yield return new WaitForSeconds(swordEffectDelayTime);

        GameObject effectPlayer = (GameObject)Instantiate(swordEffect, newPosition, combinedRotation);

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
    }

    IEnumerator HitEffectLoop(Collider other)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position); // 충돌 지점 추정
        Vector3 newPosition = hitPoint + new Vector3(0, 1, 0);

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
}
