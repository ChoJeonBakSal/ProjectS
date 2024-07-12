using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Changed : MonoBehaviour
{
    [SerializeField] private IsPlayer Human;
    [SerializeField] private IsPlayer Wolf;

    public delegate void PlayerChangedHandler();
    public static event PlayerChangedHandler OnPlayerChange;

    public void OnPlayerChanged(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            OnPlayerChange?.Invoke();

            //Human.AllChildTransformChangedLayer();
            //Wolf.AllChildTransformChangedLayer();
        }
    }

}
