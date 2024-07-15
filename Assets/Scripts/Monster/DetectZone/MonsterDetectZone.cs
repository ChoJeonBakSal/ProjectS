using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDetectZone : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;

    public event Action<Transform> OnTargetChanged;

    private void OnTriggerEnter(Collider other)
    {
        if(((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            OnTargetChanged?.Invoke(other.transform);
        }
    }
}
