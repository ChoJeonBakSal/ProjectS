using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeHitManager : MonoBehaviour
{
    [Header("Skill Damage")]
    [SerializeField] private float uSkillDamage = 50f;

    [SerializeField] private int targetLayer;

    [Header("Hit Effect Timing")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitDelayTime = 1f;
    [SerializeField] private float hitLoopTimeLimit = 2.0f;

    [Header("Hit Effect Spawn options")]
    [SerializeField] private float hitSpawnScale = 1.5f;

    [Header("Utimate Skill Effect")]
    [SerializeField] private float slowMotionDuration = 0.15f; // ���ο��� ���� �ð�
    [SerializeField] private float slowMotionFactor = 0.1f; // �ð� ������ ���� ����

    void Start()
    {
        // Ÿ�� ���̾ �����մϴ�. ���� ���, ���̾� �̸��� "Monster"���:
        targetLayer = LayerMask.NameToLayer("Monster");

        // �ӽ������� �� �ݶ��̴� ����.
        //skillCollider.enabled = false;

        //swordTrailEffect.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        //PlayerAttackManager pam = transform.GetComponent<PlayerAttackManager>();
        // �浹�� ��ü�� ���̾ Ÿ�� ���̾����� Ȯ���մϴ�.
        //if (other.gameObject.layer == targetLayer && !pam.isAttacking)
        if (other.gameObject.layer == targetLayer)
        {
            Debug.Log("Nuke Monster Collider�� �浹 ����!");

            // ���⼭ �浹 ó���� �մϴ�.
            MonsterView hitMonster = other.GetComponent<MonsterView>();
            if (hitMonster != null)
                hitMonster.HurtDamage(uSkillDamage, transform);

            StartCoroutine(HitEffectLoop(other));
            //StartCoroutine(SlowMotionEffect());
        }
    }


    IEnumerator HitEffectLoop(Collider other)
    {
        //Vector3 hitPoint = other.ClosestPoint(transform.position); // �浹 ���� ����
        Vector3 hitPoint = other.transform.position; // �浹 ���� ����

        Vector3 newPosition = hitPoint + new Vector3(0, 1, 0);

        yield return new WaitForSeconds(hitDelayTime);

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

}
