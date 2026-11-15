using System.Collections;
using TMPro;
using UnityEngine;

// Put this on an ALWAYS-ACTIVE object on the shared UI canvas (not on the inventory screen,
// which is hidden during play). Needs a TMP text (assign it) and a CanvasGroup on the same object.
[RequireComponent(typeof(CanvasGroup))]
public class InventoryNotice : MonoBehaviour
{
    public TMP_Text label;
    public InventoryManager inventory;   // optional: found automatically if left empty
    public float holdSeconds = 1.5f;
    public float fadeSeconds = 0.5f;

    CanvasGroup group;
    Coroutine running;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (inventory != null)
            inventory.OnNotice += Show;
        else
            Debug.LogWarning("InventoryNotice: no InventoryManager found.", this);
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnNotice -= Show;
    }

    void Show(string message)
    {
        if (label != null) label.text = message;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        group.alpha = 1f;
        yield return new WaitForSecondsRealtime(holdSeconds);

        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - t / fadeSeconds;
            yield return null;
        }

        group.alpha = 0f;
        running = null;
    }
}