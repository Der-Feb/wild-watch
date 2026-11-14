using UnityEngine;

public enum UIState { Playing, Menu, Inventory, Map }

// Put this on an always-active object (for example Managers), NOT on one of the panels.
// Exactly one screen is open at a time, so the inventory and the map can never overlap.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Gameplay scripts check this before reacting to keys such as E.
    public static bool IsPlaying => Instance == null || Instance.State == UIState.Playing;

    [Header("Panels (children of the UI canvas)")]
    public GameObject menuPanel;
    public GameObject inventoryPanel;
    public GameObject mapPanel;          // optional: leave empty until the map exists

    [Header("Keys")]
    public KeyCode menuKey = KeyCode.Escape;
    public KeyCode inventoryKey = KeyCode.I;
    public KeyCode mapKey = KeyCode.M;

    public UIState State { get; private set; } = UIState.Playing;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetState(UIState.Playing);
    }

    void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        Time.timeScale = 1f;   // never leave the editor paused after stopping Play mode
    }

    void Update()
    {
        // Update still runs while Time.timeScale is 0, so these keys keep working when paused.
        if (Input.GetKeyDown(menuKey))
        {
            SetState(State == UIState.Playing ? UIState.Menu : UIState.Playing);
        }
        else if (Input.GetKeyDown(inventoryKey))
        {
            SetState(State == UIState.Inventory ? UIState.Playing : UIState.Inventory);
        }
        else if (Input.GetKeyDown(mapKey))
        {
            if (mapPanel == null)
                Debug.Log("Map pressed, but no map panel is assigned yet.");
            else
                SetState(State == UIState.Map ? UIState.Playing : UIState.Map);
        }
    }

    // Hooked up to buttons in the Inspector (OnClick).
    public void Resume() => SetState(UIState.Playing);
    public void OpenMenu() => SetState(UIState.Menu);
    public void OpenInventory() => SetState(UIState.Inventory);

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetState(UIState next)
    {
        State = next;

        if (menuPanel != null) menuPanel.SetActive(next == UIState.Menu);
        if (inventoryPanel != null) inventoryPanel.SetActive(next == UIState.Inventory);
        if (mapPanel != null) mapPanel.SetActive(next == UIState.Map);

        bool playing = next == UIState.Playing;

        Time.timeScale = playing ? 1f : 0f;
        Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !playing;
    }
}