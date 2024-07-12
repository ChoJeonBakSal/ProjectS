using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Changed : MonoBehaviour
{
    private static int PlayerLayer;
    private static int PlayerSubLayer;

    [SerializeField] private Transform Camera;

    private void Awake()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlayerSubLayer = LayerMask.NameToLayer("Player_Sub");
    }

    private void Start()
    {
        if(gameObject.layer == PlayerLayer)
        {
            Camera.gameObject.SetActive(true);
        }else Camera.gameObject.SetActive(false);
    }

    public void OnPlayerChanged(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Debug.Log(GetInstanceID());

            AllChildTransformChangedLayer();
        }
    }

    private void AllChildTransformChangedLayer()
    {
        gameObject.layer = gameObject.layer == PlayerLayer ? PlayerSubLayer : PlayerLayer;
       
        if (transform.childCount > 0) ChangeLayerRecursively(transform);
        Camera.gameObject.SetActive(gameObject.layer == PlayerLayer);
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
