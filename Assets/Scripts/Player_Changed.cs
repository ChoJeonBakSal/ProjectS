using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Changed : MonoBehaviour
{
    [SerializeField] private IsPlayer Human;
    [SerializeField] private IsPlayer Wolf;

    public void OnPlayerChanged(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Human.AllChildTransformChangedLayer();
            Wolf.AllChildTransformChangedLayer();
        }
    }

}
