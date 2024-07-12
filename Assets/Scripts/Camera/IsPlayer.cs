using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsPlayer : MonoBehaviour
{
    private static int PlayerLayer;
    private static int PlayerSubLayer;

    public bool isPlayer;

    [SerializeField] private GameObject Camera;

    private void Awake()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlayerSubLayer = LayerMask.NameToLayer("Player_Sub");
    }

    private void Start()
    {
        if (gameObject.layer == PlayerLayer)
        {
            isPlayer = true;
        }

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
