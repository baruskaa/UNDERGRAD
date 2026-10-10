using UnityEngine;

public class SaveableWorldObject : MonoBehaviour
{
    [SerializeField]
    private string uniqueID;
    public string UniqueID => uniqueID;

    public bool IsStateChanged { get; private set; } = false;

    [ContextMenu("Generate Unique ID")]
    public void GenerateID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            GenerateID();
        }
    }

    public void SetStateChanged(bool changed)
    {
        IsStateChanged = changed;
    }

    public void RestoreState(WorldObjectState state)
    {
        if (state == null) return;

        IsStateChanged = state.isInteractableStateChanged;

        // 1. Check for UnlockHallwayDoor
        UnlockHallwayDoor hallwayDoor = GetComponent<UnlockHallwayDoor>();
        if (hallwayDoor != null)
        {
            hallwayDoor.ApplyLoadedState(state.isInteractableStateChanged);
            return;
        }

        // 2. NEW: Check for BathroomDoor (e.g., Door_CRGirls_3)
        BathroomDoor bathroomDoor = GetComponent<BathroomDoor>();
        if (bathroomDoor != null)
        {
            bathroomDoor.ApplyLoadedState(state.isInteractableStateChanged);
            return;
        }

        // 3. Fallback for scriptless design objects (Tilemaps, props, etc.)
        gameObject.SetActive(state.isActive);
    }
}