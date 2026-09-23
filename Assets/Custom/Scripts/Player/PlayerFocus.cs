using UnityEngine;

public class PlayerFocus : MonoBehaviour
{
    public PlayerCamera playerCamera;
    public GameObject focusUI;
    public Renderer[] bodyRenderersToHide; // assign your Cylinder's Renderer here

    void Update()
    {
        bool focusing = Input.GetKey(KeyCode.RightShift);

        playerCamera.isFocusing = focusing;

        if (focusUI != null)
            focusUI.SetActive(focusing);

        foreach (var r in bodyRenderersToHide)
        {
            if (r != null)
                r.enabled = !focusing;
        }
    }
}