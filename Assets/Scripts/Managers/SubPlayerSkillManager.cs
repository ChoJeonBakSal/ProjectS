using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPlayerSkillManager : MonoBehaviour
{
    [Header("Skill Object List")]
    [SerializeField] private GameObject Wolfs;
    [SerializeField] private List<GameObject> wolfs_Anims = new List<GameObject>();
    //[SerializeField] private List<GameObject> bloomObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> afterEffect = new List<GameObject>();

    [Header("Player variable")]
    [SerializeField] private Animator p_Anim;
    [SerializeField] public bool isCasting = false; // 애니메이션 실행중 재입력 불가

    [Header("Hit Effect")]
    [SerializeField] private BoxCollider SkillColi;
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

    [Header("Player Skill Material")]
    [SerializeField] private Material ori_Material;
    [SerializeField] private Material effect_Material;

    void Start()
    {
        // 타겟 레이어를 설정합니다. 예를 들어, 레이어 이름이 "Monster"라면:
        targetLayer = LayerMask.NameToLayer("Monster");

        // 임시적으로 물기 콜라이더 끄기.
        SkillColi.enabled = false;

        biteTrailEffect.SetActive(false);

        Wolfs.SetActive(false);

        AfterEffectOff();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isCasting)
        {
            OnCasting();
        }
    }

    void OnCasting()
    {
        Wolfs.SetActive(true);

        AfterEffectOn();

        OnChangeMaterial();

        isCasting = true;  // 애니메이션 시작 시 공격 상태로 전환

        // 공격 애니메이션 트리거 실행
        p_Anim.SetTrigger("BasicSkill");
        // Wolfs의 애니메이션 foreach문의 실행.
        PlayAnimator();

        SkillColi.enabled = true;

        //StartCoroutine(MoveForwardByDiameter());

        StartCoroutine(SwordEffectLoop());
    }

    void OnTriggerEnter(Collider other)
    {
        SubPlayerAttackManager psm = transform.GetComponent<SubPlayerAttackManager>();
        // 충돌한 객체의 레이어가 타겟 레이어인지 확인합니다.
        //if (other.gameObject.layer == targetLayer)
        if (other.gameObject.layer == targetLayer && !psm.isAttacking)
        {
            Debug.Log("Monster Collider와 충돌 감지!");

            // 여기서 충돌 처리를 합니다.
            MonsterView hitMonster = other.GetComponent<MonsterView>();
            hitMonster.HurtDamage(150, transform);

            StartCoroutine(HitEffectLoop(other));
            StartCoroutine(SlowMotionEffect());
        }
    }

    IEnumerator SwordEffectLoop()
    {
        biteTrailEffect.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        SkillColi.enabled = false;

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

        Wolfs.SetActive(false);

        OffChangeMaterial();

        AfterEffectOff();

        isCasting = false;  // 공격 애니메이션이 끝난 후 공격 상태 해제

        biteTrailEffect.SetActive(false);
    }

    IEnumerator HitEffectLoop(Collider other)
    {
        yield return new WaitForSeconds(hitEffectDalayTime);

        Vector3 hitPoint = other.ClosestPoint(transform.position); // 충돌 지점 추정
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

    void PlayAnimator()
    {
        foreach(GameObject obj in wolfs_Anims)
        {
            Animator wolfs_Anims = obj.GetComponent<Animator>();
            wolfs_Anims.SetTrigger("BasicSkill");
        }
    }
    
    void AfterEffectOff()
    {
        foreach(GameObject obj in afterEffect)
        {
            obj.SetActive(false);
        }
    }

    void AfterEffectOn()
    {
        foreach(GameObject obj in afterEffect)
        {
            obj.SetActive(true);
        }
    }

    void OnChangeMaterial()
    {
        // 현재 GameObject의 자식들 중 Skinned Mesh Renderer를 찾아 Material을 변경
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var renderer in skinnedMeshRenderers)
        {
            renderer.material = effect_Material;
        }
    }

    void OffChangeMaterial()
    {
        // 현재 GameObject의 자식들 중 Skinned Mesh Renderer를 찾아 Material을 변경
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var renderer in skinnedMeshRenderers)
        {
            renderer.material = ori_Material;
        }
    }
}
