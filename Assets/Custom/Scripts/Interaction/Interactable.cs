using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Info")]
    public string interactableName = "Object";

    static readonly Dictionary<string, int> nameCounts = new Dictionary<string, int>();

    public string DisplayName { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => nameCounts.Clear();

    protected virtual void Awake()
    {
        nameCounts.TryGetValue(interactableName, out int count);
        count++;
        nameCounts[interactableName] = count;
        DisplayName = $"{interactableName} #{count}";
    }

    public virtual void Interact()
    {
        Debug.Log($"Interacted with {DisplayName}");
    }
}