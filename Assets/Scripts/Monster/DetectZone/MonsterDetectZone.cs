using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterDetectZone : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;

    private Transform target;

    public event Action<Transform> OnTargetChanged;

    private void OnTriggerEnter(Collider other)
    {
        if(((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            if (target != null) return;

            target = other.transform;
            OnTargetChanged?.Invoke(target);
            //targets.Add(other.transform);
            //OnTargetChanged?.Invoke(other.transform);
        }
    }
}
