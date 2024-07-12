using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsPlayer : MonoBehaviour
{
    private static int PlayerLayer;
    private static int PlayerSubLayer;

    public bool isPlayer { get; private set; }
    [SerializeField] private GameObject Camera;

    private void Awake()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlayerSubLayer = LayerMask.NameToLayer("Player_Sub");

        Player_Changed.OnPlayerChange += AllChildTransformChangedLayer;
    }

    private void Start()
    {
        isPlayer = gameObject.layer == PlayerLayer;
        Camera.SetActive(isPlayer);
    }

    public void AllChildTransformChangedLayer()
    {
        isPlayer = gameObject.layer == PlayerLayer? false : true;
        Camera.SetActive(isPlayer);
        gameObject.layer = gameObject.layer == PlayerLayer ? PlayerSubLayer : PlayerLayer;

        if (transform.childCount > 0) ChangeLayerRecursively(transform);
    }

    private void ChangeLayerRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            int newLayer = child.gameObject.layer == PlayerLayer ? PlayerSubLayer : PlayerLayer;
            child.gameObject.layer = newLayer;
            ChangeLayerRecursively(child);
        }
    }
}
