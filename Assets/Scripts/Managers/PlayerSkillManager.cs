using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Player variable")]
    [SerializeField] private Animator p_Anim;
    [SerializeField] private bool isCasting = false; // �ִϸ��̼� ������ ���Է� �Ұ�


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


    void Start()
    {
        // Ÿ�� ���̾ �����մϴ�. ���� ���, ���̾� �̸��� "Monster"���:
        targetLayer = LayerMask.NameToLayer("Monster");

        // �ӽ������� �� �ݶ��̴� ����.
        skillCollider.enabled = false;

        swordTrailEffect.SetActive(false);
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
        swordTrailEffect.SetActive(true);

        isCasting = true;  // �ִϸ��̼� ���� �� ���� ���·� ��ȯ

        //skillCollider.enabled = false;

        // ���� �ִϸ��̼� Ʈ���� ����
        p_Anim.SetTrigger("BasicSkill");
        Time.timeScale = beforeSkillEffectDelayTimeScale;
    }

    void OnTriggerEnter(Collider other)
    {
        // �浹�� ��ü�� ���̾ Ÿ�� ���̾����� Ȯ���մϴ�.
        if (other.gameObject.layer == targetLayer)
        {
            Debug.Log("Monster Collider�� �浹 ����!");

            // ���⼭ �浹 ó���� �մϴ�.
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
        skillCollider.enabled = true;
        StartCoroutine(OffTrigger());
    }

    IEnumerator OffTrigger()
    {
        StartCoroutine(SkillEffectLoop());
        yield return new WaitForSeconds(0.1f);
        skillCollider.enabled = false;
    }
}
