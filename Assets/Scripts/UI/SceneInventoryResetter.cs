using UnityEngine;
using Inventory.Model;

public class SceneInventoryResetter : MonoBehaviour
{
    [Header("Inventory Reference")]
    [Tooltip("Drag your InventorySO asset here in the Inspector.")]
    [SerializeField] private InventorySO playerInventory;

    [Header("Reset Settings")]
    [Tooltip("Determines whether to clear inventory in Awake (earlier) or Start (later).")]
    [SerializeField] private ExecutionTiming executionTiming = ExecutionTiming.Awake;

    private enum ExecutionTiming
    {
        Awake,
        Start
    }

    private void Awake()
    {
        if (executionTiming == ExecutionTiming.Awake)
        {
            ResetPlayerInventory();
        }
    }

    private void Start()
    {
        if (executionTiming == ExecutionTiming.Start)
        {
            ResetPlayerInventory();
        }
    }

    public void ResetPlayerInventory()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[SceneInventoryResetter] Player Inventory reference is missing! Please assign it in the Inspector.");
            return;
        }

        // Ensure the inventory slots exist first
        playerInventory.Initialize();

        // Completely clears all slots and invokes OnInventoryUpdated
        playerInventory.ClearInventory();

        Debug.Log("<color=cyan>[SceneInventoryResetter]</color> Player inventory wiped completely on scene load.");
    }
}