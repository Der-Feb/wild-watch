using TMPro;
using UnityEngine;

// Put this on an empty object with a Collider set to "Is Trigger".
// Children: a pivot holding the big display model, and an empty "LabelAnchor" above it.
// The count label itself is created on the shared UI canvas (see WorldLabelLayer).
// This does NOT use the raycast detector: it works by walking into the trigger.
public class ItemStation : MonoBehaviour
{
    [Header("Stock")]
    public ItemData item;
    public int stock = 5;

    [Header("Player")]
    public InventoryManager inventory;   // optional: found automatically if left empty
    public string playerTag = "Player";  // the root object of the player must have this tag

    [Header("Display")]
    public Transform visual;         // the big display model (no Pickup, no colliders on it)
    public float spinSpeed = 60f;    // degrees per second
    public float bobHeight = 0.25f;
    public float bobSpeed = 2f;

    [Header("Label (screen space)")]
    public Transform labelAnchor;        // empty object above the model; the label follows it
    public float labelMaxDistance = 25f; // hide the label beyond this distance

    [Header("Input")]
    public KeyCode takeKey = KeyCode.E;

    [Header("Dropped stations")]
    public bool destroyWhenEmpty;        // Setup() turns this on; leave OFF for the hand-placed stations
    public float dropVisualScale = 1f;   // multiplies the item's worldPrefab scale

    int collidersInside;             // the player may have several colliders
    bool PlayerInside => collidersInside > 0;

    Vector3 visualStartLocalPos;
    Camera cam;
    TMP_Text label;

    // Called right after Instantiate (before Start) by InventoryManager.Drop.
    public void Setup(ItemData newItem, int newStock)
    {
        item = newItem;
        stock = Mathf.Max(0, newStock);
        destroyWhenEmpty = true;

        if (visual != null && item != null && item.worldPrefab != null)
        {
            foreach (Transform child in visual)
                Destroy(child.gameObject);

            GameObject model = Instantiate(item.worldPrefab, visual);   // keeps the prefab's own scale
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale *= dropVisualScale;

            // The display model must not be pickable or solid.
            // Interactable is the base class of the pickup script, so this works whatever it is called.
            foreach (var c in model.GetComponentsInChildren<Collider>()) Destroy(c);
            foreach (var p in model.GetComponentsInChildren<Interactable>()) Destroy(p);
        }

        Refresh();
    }

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
            Debug.LogWarning("No camera tagged MainCamera; the station label will not show.", this);

        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory == null)
            Debug.LogWarning("No InventoryManager found; this station cannot give items.", this);

        if (visual != null) visualStartLocalPos = visual.localPosition;
        stock = Mathf.Max(0, stock);

        if (WorldLabelLayer.Instance != null)
            label = WorldLabelLayer.Instance.Create($"{name} label");
        else
            Debug.LogWarning("No WorldLabelLayer in the scene; the station label is disabled.", this);

        Refresh();
    }

    void OnDestroy()
    {
        if (label != null) Destroy(label.gameObject);
    }

    void Update()
    {
        if (stock <= 0) return;

        if (visual != null)
        {
            visual.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

            Vector3 p = visualStartLocalPos;
            p.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            visual.localPosition = p;
        }

        UpdateLabelPosition();

        // UIManager.IsPlaying: no taking items while the menu or inventory is open.
        if (PlayerInside && UIManager.IsPlaying && Input.GetKeyDown(takeKey))
            TryTake();
    }

    void UpdateLabelPosition()
    {
        if (label == null || cam == null) return;

        Vector3 worldPos = labelAnchor != null
            ? labelAnchor.position
            : transform.position + Vector3.up * 2f;

        Vector3 screen = cam.WorldToScreenPoint(worldPos);

        bool inFront = screen.z > 0f;   // behind the camera would draw a mirrored label
        bool onScreen = screen.x >= 0f && screen.x <= Screen.width
                     && screen.y >= 0f && screen.y <= Screen.height;
        bool near = (cam.transform.position - worldPos).sqrMagnitude
                    <= labelMaxDistance * labelMaxDistance;

        bool visible = inFront && onScreen && near;

        if (label.gameObject.activeSelf != visible)
            label.gameObject.SetActive(visible);

        if (visible)
            label.rectTransform.position = screen;   // overlay canvas: position is in screen pixels
    }

    void TryTake()
    {
        if (stock <= 0 || item == null) return;

        if (inventory == null)
        {
            Debug.LogWarning($"{name}: no InventoryManager, nothing taken.", this);
            return;
        }

        // Spend stock only if the inventory actually accepted the item.
        // (When it refuses, the manager raises the "Inventory full" / "Ammo full" notice.)
        if (!inventory.Add(item))
        {
            Debug.Log($"{name}: inventory full, nothing taken.");
            return;
        }

        stock = Mathf.Max(0, stock - 1);
        Debug.Log($"{name}: took 1 {item.itemName}, {stock} left.");
        Refresh();

        if (stock <= 0 && destroyWhenEmpty)
            Destroy(gameObject);   // only dropped stations; OnDestroy removes the label
    }

    public void Restock(int amount)
    {
        stock = Mathf.Max(0, stock + amount);
        Refresh();
    }

    bool IsPlayer(Collider other)
    {
        return other.transform.root.CompareTag(playerTag);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        collidersInside++;
        if (collidersInside == 1)
            Debug.Log($"{name}: player entered the station area.");
        Refresh();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        collidersInside = Mathf.Max(0, collidersInside - 1);
        Refresh();
    }

    // Updates the text and hides everything at zero stock.
    // Whether the label is on screen is decided every frame in UpdateLabelPosition.
    void Refresh()
    {
        bool available = stock > 0;

        if (visual != null) visual.gameObject.SetActive(available);

        if (label == null) return;

        if (!available)
        {
            label.gameObject.SetActive(false);
            return;
        }

        string itemName = item != null ? item.itemName : "Item";
        label.text = $"{itemName} x{stock}";
        if (PlayerInside)
            label.text += $"\n[{takeKey}] Take";
    }
}