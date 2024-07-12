using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform camera;
    void Update()
    {
        transform.position = camera.position;
        transform.rotation = camera.rotation;
    }
}
