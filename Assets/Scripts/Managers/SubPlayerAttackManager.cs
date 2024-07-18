using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPlayerAttackManager : MonoBehaviour
{
    [Header("Hit Damage")]
    [SerializeField] private float hitDamage = 20f;

    [Header("Player variable")]
    [SerializeField] private Animator p_Anim;
    [SerializeField] public bool isAttacking = false; // �ִϸ��̼� ������ ���Է� �Ұ�

    [Header("Hit Effect")]
    [SerializeField] private BoxCollider biteColi;
    [SerializeField] private int targetLayer;
    [SerializeField] private float hitEffectDalayTime; // ���ο��� ���� �ð�
    [SerializeField] private float slowMotionDuration; // ���ο��� ���� �ð�
    [SerializeField] private float slowMotionFactor; // �ð� ������ ���� ����

    [Header("Hit Effect Timing")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitLoopTimeLimit = 2.0f;

    [Header("Hit Effect Spawn options")]
    [SerializeField] private float hitSpawnScale = 1.0f;

    [Header("Bite Effect Timing")]
    [SerializeField] private GameObject biteTrailEffect;

    [Header("Move Forward Timing")]
    [SerializeField] private float totalDistance; // �̵��� �� �Ÿ�
    [SerializeField] private float moveForwardTime; // �̵� �ð�
    void Start()
    {
        // Ÿ�� ���̾ �����մϴ�. ���� ���, ���̾� �̸��� "Monster"���:
        targetLayer = LayerMask.NameToLayer("Monster");

        // �ӽ������� �� �ݶ��̴� ����.
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
            isAttacking = true;  // �ִϸ��̼� ���� �� ���� ���·� ��ȯ

            // ���� �ִϸ��̼� Ʈ���� ����
            p_Anim.SetTrigger("BasicAtk");

            biteColi.enabled = true;

            //StartCoroutine(MoveForwardByDiameter());

            StartCoroutine(SwordEffectLoop());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //if (!other.TryGetComponent<SphereCollider>(out SphereCollider sphereCollider))
        //    return;

        SubPlayerSkillManager psm = transform.GetComponent<SubPlayerSkillManager>();
        // �浹�� ��ü�� ���̾ Ÿ�� ���̾����� Ȯ���մϴ�.
        //if (other.gameObject.layer == targetLayer)
        if (other.gameObject.layer == targetLayer && !psm.isCasting)
        {
            Debug.Log("Monster Collider�� �浹 ����!");

            // ���⼭ �浹 ó���� �մϴ�.
            MonsterView hitMonster = other.GetComponent<MonsterView>();
            if(hitMonster != null)
                hitMonster.HurtDamage(hitDamage, transform);

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

        // ������ ��ġ ����
        transform.position = targetPosition;

        yield return new WaitForSeconds(hitLoopTimeLimit);

        isAttacking = false;  // ���� �ִϸ��̼��� ���� �� ���� ���� ����

        biteTrailEffect.SetActive(false);
    }

    IEnumerator HitEffectLoop(Collider other)
    {
        yield return new WaitForSeconds(hitEffectDalayTime);
        
        //Vector3 hitPoint = other.ClosestPoint(transform.position); // �浹 ���� ����
        Vector3 hitPoint = other.transform.position; // �浹 ���� ����
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

    // ���⿡ transform rotation�� �������� transform position�� ����1��ŭ�� ũ�⸸ŭ ������ �����ϴ� �޼��� �ۼ�����
    IEnumerator MoveForwardByDiameter()
    {
        //float totalDistance = 6.0f; // �̵��� �� �Ÿ�
        //float duration = 0.8f; // �̵��ϴ� �� �ɸ��� �ð�
        float elapsed = 0.0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + transform.forward * totalDistance;

        while (elapsed < moveForwardTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / moveForwardTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ������ ��ġ ����
        transform.position = targetPosition;
    }

}
