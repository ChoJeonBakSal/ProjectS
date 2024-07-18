using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtimateSkillManager : MonoBehaviour
{
    [Header("Nuke Object")]
    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject Nuclear;
    [SerializeField] private GameObject UtimatePlane;
    [SerializeField] private GameObject TimeLine;
    //[SerializeField] private Animator p_Anim;
    //[SerializeField] public bool isCasting = false; // 애니메이션 실행중 재입력 불가

    [Header("Utimate Skill Effect")]
    [SerializeField] private CapsuleCollider skillCollider;
    [SerializeField] private Transform spawnPoint;
    
    [SerializeField] private float nukeSpawnDelayTime = 5f; // 스폰 지연시간
    [SerializeField] private float nukeDestroyDelayTime = 2f; // 스폰 지연시간

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnSkill();
        }
    }

    void OnSkill()
    {
        // 컷씬 On / Off
        StartCoroutine(CutSceneLoading());
        // Nuke Spawn
        StartCoroutine(SpawnNukeInBattleField());

    }

    IEnumerator CutSceneLoading()
    {
        CinemachineBrain brain = MainCamera.GetComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;

        UtimatePlane.SetActive(true);
        TimeLine.SetActive(true);
        yield return new WaitForSecondsRealtime(4.5f);
        UtimatePlane.SetActive(false);
        TimeLine.SetActive(false);

        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
    }

    IEnumerator SpawnNukeInBattleField()
    {
        yield return new WaitForSecondsRealtime(nukeSpawnDelayTime);

        Vector3 spawnVector = this.spawnPoint.position;

        GameObject nukeObj = (GameObject)Instantiate(Nuclear, spawnVector, transform.rotation);

        // Collider Control.
        skillCollider = nukeObj.GetComponent<CapsuleCollider>();
        skillCollider.enabled = false;

        Rigidbody rb = nukeObj.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;

        yield return new WaitForSecondsRealtime(5f);
        skillCollider.enabled = true;
        Debug.Log("Nuke Collider On 1");
        yield return new WaitForSecondsRealtime(1f);
        skillCollider.enabled = false;
        Debug.Log("Nuke Collider Off 1");

        yield return new WaitForSecondsRealtime(1f);
        skillCollider.enabled = true;
        Debug.Log("Nuke Collider On 2");
        yield return new WaitForSecondsRealtime(1f);
        skillCollider.enabled = false;
        Debug.Log("Nuke Collider Off 2");

        yield return new WaitForSecondsRealtime(1f);
        skillCollider.enabled = true;
        Debug.Log("Nuke Collider On 3");
        yield return new WaitForSecondsRealtime(1f);
        skillCollider.enabled = false;
        Debug.Log("Nuke Collider Off 3");

        yield return new WaitForSecondsRealtime(nukeDestroyDelayTime);

        Destroy(nukeObj);
    }
}
