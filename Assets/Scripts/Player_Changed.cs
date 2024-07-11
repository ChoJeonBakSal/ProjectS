using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Changed : MonoBehaviour
{
    private static int PlayerLayer;
    private static int PlayerSubLayer;

    private void Awake()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlayerSubLayer = LayerMask.NameToLayer("Player_Sub");
    }
    public void OnPlayerChanged(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            gameObject.layer = gameObject.layer == PlayerLayer ? PlayerSubLayer : PlayerLayer;
        }
    }
}
