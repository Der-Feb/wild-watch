using TMPro;
using UnityEngine;

// Put this on an empty child of the shared UI canvas (for example UI/WorldLabels).
// Stations ask it for a label; the labels are positioned on screen by the stations themselves.
public class WorldLabelLayer : MonoBehaviour
{
    public static WorldLabelLayer Instance { get; private set; }

    public TMP_Text labelPrefab;   // a UI TextMeshPro text, centered, saved as a prefab

    void Awake()
    {
        Instance = this;
    }

    public TMP_Text Create(string labelName)
    {
        TMP_Text label = Instantiate(labelPrefab, transform);
        label.name = labelName;
        label.raycastTarget = false;          // never block clicks
        label.gameObject.SetActive(false);    // the station decides when it is visible
        return label;
    }
}