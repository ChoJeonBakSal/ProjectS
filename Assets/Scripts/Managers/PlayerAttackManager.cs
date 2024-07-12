using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Hit Effect")]
    [SerializeField] private BoxCollider swordColi;
    [SerializeField] private int targetLayer;
    [SerializeField] public float slowMotionDuration; // 슬로우모션 지속 시간
    [SerializeField] public float slowMotionFactor; // 시간 느리게 가는 정도

    [Header("Effect Timing")]
    public GameObject chosenEffect;
    private Quaternion additionalRotation = Quaternion.Euler(90, 0, -90);
    public float loopTimeLimit = 2.0f;

    [Header("Spawn options")]
    public bool disableLights = true;
    public bool disableSound = true;
    public float spawnScale = 1.0f;


    void Start()
    {
        // 타겟 레이어를 설정합니다. 예를 들어, 레이어 이름이 "Monster"라면:
        targetLayer = LayerMask.NameToLayer("Monster");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnAttack();
        }
    }

    void OnAttack()
    {
        StartCoroutine("EffectLoop");
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌한 객체의 레이어가 타겟 레이어인지 확인합니다.
        if (other.gameObject.layer == targetLayer)
        {
            Debug.Log("Monster Collider와 충돌 감지!");
            // 여기서 충돌 처리를 합니다.
            StartCoroutine(SlowMotionEffect());
        }
    }

    private IEnumerator SlowMotionEffect()
    {
        Time.timeScale = slowMotionFactor;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
    }


    IEnumerator EffectLoop()
    {
        Quaternion combinedRotation = transform.rotation * additionalRotation;
        Vector3 newPosition = transform.position + new Vector3(0, 1, 0);
        GameObject effectPlayer = (GameObject)Instantiate(chosenEffect, newPosition, combinedRotation);

        effectPlayer.transform.localScale = new Vector3(spawnScale, spawnScale, spawnScale);

        if (disableLights && effectPlayer.GetComponent<Light>())
        {
            effectPlayer.GetComponent<Light>().enabled = false;
        }

        if (disableSound && effectPlayer.GetComponent<AudioSource>())
        {
            effectPlayer.GetComponent<AudioSource>().enabled = false;
        }

        yield return new WaitForSeconds(loopTimeLimit);

        Destroy(effectPlayer);
    }
}
