using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Info")]
    public string interactableName = "Object";

    // Tracks how many instances of each name have been registered so far
    private static Dictionary<string, int> nameCounts = new Dictionary<string, int>();

    // The unique display name assigned to this specific instance
    public string DisplayName { get; private set; }

    protected virtual void Awake()
    {
        RegisterName();
    }

    void RegisterName()
    {
        if (!nameCounts.ContainsKey(interactableName))
        {
            nameCounts[interactableName] = 0;
        }

        nameCounts[interactableName]++;
        int number = nameCounts[interactableName];

        DisplayName = $"{interactableName} #{number}";
    }

    // Later: this is where you'd trigger pickup/open/use logic
    public virtual void Interact()
    {
        Debug.Log($"Interacted with {DisplayName}");
    }
}