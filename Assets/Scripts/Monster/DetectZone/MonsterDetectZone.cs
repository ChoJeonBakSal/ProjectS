using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterDetectZone : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;

    private List<Transform> targets = new List<Transform>();

    private event Action OnTargetCountChanged;
    public event Action<Transform> OnTargetChanged;

    private void OnEnable()
    {
        targets.Clear();
        OnTargetCountChanged += MonsterTargetPriority;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            targets.Add(other.transform);
            //OnTargetChanged?.Invoke(other.transform);
        }
    }

    private void Update()
    {
        MonsterTargetPriority();
    }

    private void MonsterTargetPriority()
    {
        if(targets.Count <= 0) return;

        if(targets.Count > 1)
        {
            OnTargetChanged?.Invoke(FindClosestTarget());
        }
        else
        {
            OnTargetChanged?.Invoke(targets.First());
        }
    }

    private Transform FindClosestTarget()
    {
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach(Transform t in targets)
        {
            float distance = Vector3.Distance(t.position, transform.position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = t;
            }
        }

        return closestTarget;
    }
}
