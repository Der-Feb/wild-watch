using UnityEngine;

public class PlayerFocus : MonoBehaviour
{
    public PlayerCamera playerCamera;
    public GameObject focusUI;
    public GameObject simpleFocusUI;
    public Renderer[] bodyRenderersToHide; // assign your Cylinder's Renderer here

    void Update()
    {
        bool focusing = Input.GetKey(KeyCode.RightShift);

        playerCamera.isFocusing = focusing;

        if (simpleFocusUI != null)
            simpleFocusUI.SetActive(focusing);

        // will show on focus + gun which is focusable like sniper, ...
        // and then we block simpleFocusUI
        if (focusUI != null && false)
            focusUI.SetActive(focusing);


        foreach (var r in bodyRenderersToHide)
        {
            if (r != null)
                r.enabled = !focusing;
        }
    }
}